using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using VehicleRentalManagement.Models;

namespace VehicleRentalManagement.DataAccess.Repositories
{
    public class VehicleRepository : IRepository<Vehicle>
    {
        private readonly DatabaseConnection _db;
        private readonly AuditLogRepository _auditLog;

        public VehicleRepository(DatabaseConnection db)
        {
            _db = db;
            _auditLog = new AuditLogRepository(db.ConnectionString);
        }

        public IEnumerable<Vehicle> GetAll()
        {
            var vehicles = new List<Vehicle>();

            try
            {
                using (var conn = _db.GetConnection())
                {
                    var query = @"SELECT v.*, 
                                 u1.FullName as CreatedByName, 
                                 u2.FullName as ModifiedByName
                                 FROM Vehicles v
                                 LEFT JOIN Users u1 ON v.CreatedBy = u1.UserId
                                 LEFT JOIN Users u2 ON v.ModifiedBy = u2.UserId
                                 WHERE v.IsActive = 1
                                 ORDER BY v.VehicleName";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vehicles.Add(MapToVehicle(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new Exception("Araç listesi yüklenirken hata oluştu: " + ex.Message, ex);
            }

            return vehicles;
        }

        public Vehicle GetById(int id)
        {
            Vehicle vehicle = null;

            using (var conn = _db.GetConnection())
            {
                var query = @"SELECT v.*, 
                             u1.FullName as CreatedByName, 
                             u2.FullName as ModifiedByName
                             FROM Vehicles v
                             LEFT JOIN Users u1 ON v.CreatedBy = u1.UserId
                             LEFT JOIN Users u2 ON v.ModifiedBy = u2.UserId
                             WHERE v.VehicleId = @VehicleId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehicle = MapToVehicle(reader);
                        }
                    }
                }
            }

            return vehicle;
        }

        public int Add(Vehicle entity)
        {
            int newId;
            using (var conn = _db.GetConnection())
            {
                var query = @"INSERT INTO Vehicles (VehicleName, LicensePlate, IsActive, CreatedBy, CreatedDate)
                             VALUES (@VehicleName, @LicensePlate, @IsActive, @CreatedBy, @CreatedDate);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleName", entity.VehicleName);
                    cmd.Parameters.AddWithValue("@LicensePlate", entity.LicensePlate);
                        cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
                    cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    conn.Open();
                    newId = (int)cmd.ExecuteScalar();
                }
            }

            // Audit Log
            try
            {
                var newValues = $"Araç Adı: {entity.VehicleName}, Plaka: {entity.LicensePlate}";
                _auditLog.LogAction("Vehicles", newId, "INSERT", null, newValues, entity.CreatedBy);
            }
            catch { /* Audit log hatası ana işlemi etkilememeli */ }

            return newId;
        }

        public bool Update(Vehicle entity)
        {
            // Audit log için eski değerleri al
            var oldVehicle = GetById(entity.VehicleId);

            bool result;
            using (var conn = _db.GetConnection())
            {
                var query = @"UPDATE Vehicles 
                             SET VehicleName = @VehicleName, 
                                 LicensePlate = @LicensePlate,
                                 ModifiedBy = @ModifiedBy,
                                 ModifiedDate = @ModifiedDate
                             WHERE VehicleId = @VehicleId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", entity.VehicleId);
                    cmd.Parameters.AddWithValue("@VehicleName", entity.VehicleName);
                    cmd.Parameters.AddWithValue("@LicensePlate", entity.LicensePlate);
                    cmd.Parameters.AddWithValue("@ModifiedBy", entity.ModifiedBy);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    conn.Open();
                    result = cmd.ExecuteNonQuery() > 0;
                }
            }

            // Audit Log
            if (result && oldVehicle != null && entity.ModifiedBy.HasValue)
            {
                try
                {
                    var oldValues = $"Araç Adı: {oldVehicle.VehicleName}, Plaka: {oldVehicle.LicensePlate}";
                    var newValues = $"Araç Adı: {entity.VehicleName}, Plaka: {entity.LicensePlate}";
                    _auditLog.LogAction("Vehicles", entity.VehicleId, "UPDATE", oldValues, newValues, entity.ModifiedBy.Value);
                }
                catch { /* Audit log hatası ana işlemi etkilememeli */ }
            }

            return result;
        }

        // Interface'den gelen Delete metodu - audit log olmadan
        public bool Delete(int id)
        {
            using (var conn = _db.GetConnection())
            {
                var query = "UPDATE Vehicles SET IsActive = 0 WHERE VehicleId = @VehicleId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", id);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Audit log ile silme metodu (overload)
        public bool Delete(int id, int deletedBy)
        {
            // Audit log için eski değerleri al
            var vehicle = GetById(id);

            bool result;
            using (var conn = _db.GetConnection())
            {
                var query = "UPDATE Vehicles SET IsActive = 0 WHERE VehicleId = @VehicleId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", id);
                    conn.Open();
                    result = cmd.ExecuteNonQuery() > 0;
                }
            }

            // Audit Log
            if (result && vehicle != null)
            {
                try
                {
                    var oldValues = $"Araç Adı: {vehicle.VehicleName}, Plaka: {vehicle.LicensePlate}, Durum: Aktif";
                    var newValues = $"Araç Adı: {vehicle.VehicleName}, Plaka: {vehicle.LicensePlate}, Durum: Pasif (Silindi)";
                    _auditLog.LogAction("Vehicles", id, "DELETE", oldValues, newValues, deletedBy);
                }
                catch { /* Audit log hatası ana işlemi etkilememeli */ }
            }

            return result;
        }

        private Vehicle MapToVehicle(SqlDataReader reader)
        {
            return new Vehicle
            {
                VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                VehicleName = reader.GetString(reader.GetOrdinal("VehicleName")),
                LicensePlate = reader.GetString(reader.GetOrdinal("LicensePlate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ModifiedBy")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? "" : reader.GetString(reader.GetOrdinal("CreatedByName")),
                ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? "" : reader.GetString(reader.GetOrdinal("ModifiedByName"))
            };
        }
    }
}