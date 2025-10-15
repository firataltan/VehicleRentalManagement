using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using VehicleRentalManagement.Models;
using VehicleRentalManagement.Models.ViewModels;

namespace VehicleRentalManagement.DataAccess.Repositories
{
    public class WorkingHourRepository : IRepository<WorkingHour>
    {
        private readonly DatabaseConnection _db;

        public WorkingHourRepository()
        {
            _db = new DatabaseConnection();
        }

        public IEnumerable<WorkingHour> GetAll()
        {
            var workingHours = new List<WorkingHour>();

            using (var conn = _db.GetConnection())
            {
                var query = @"SELECT wh.*, v.VehicleName, v.LicensePlate,
                             u1.FullName as CreatedByName, u2.FullName as ModifiedByName
                             FROM WorkingHours wh
                             INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
                             LEFT JOIN Users u1 ON wh.CreatedBy = u1.UserId
                             LEFT JOIN Users u2 ON wh.ModifiedBy = u2.UserId
                             ORDER BY wh.RecordDate DESC, v.VehicleName";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            workingHours.Add(MapToWorkingHour(reader));
                        }
                    }
                }
            }

            return workingHours;
        }

        public WorkingHour GetById(int id)
        {
            WorkingHour workingHour = null;

            using (var conn = _db.GetConnection())
            {
                var query = @"SELECT wh.*, v.VehicleName, v.LicensePlate,
                             u1.FullName as CreatedByName, u2.FullName as ModifiedByName
                             FROM WorkingHours wh
                             INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
                             LEFT JOIN Users u1 ON wh.CreatedBy = u1.UserId
                             LEFT JOIN Users u2 ON wh.ModifiedBy = u2.UserId
                             WHERE wh.WorkingHourId = @WorkingHourId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkingHourId", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            workingHour = MapToWorkingHour(reader);
                        }
                    }
                }
            }

            return workingHour;
        }

        public int Add(WorkingHour entity)
        {
            using (var conn = _db.GetConnection())
            {
                var query = @"INSERT INTO WorkingHours 
                             (VehicleId, RecordDate, ActiveWorkingHours, MaintenanceHours, Notes, CreatedBy, CreatedDate)
                             VALUES (@VehicleId, @RecordDate, @ActiveWorkingHours, @MaintenanceHours, @Notes, @CreatedBy, @CreatedDate);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", entity.VehicleId);
                    cmd.Parameters.AddWithValue("@RecordDate", entity.RecordDate);
                    cmd.Parameters.AddWithValue("@ActiveWorkingHours", entity.ActiveWorkingHours);
                    cmd.Parameters.AddWithValue("@MaintenanceHours", entity.MaintenanceHours);
                    cmd.Parameters.AddWithValue("@Notes", (object)entity.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool Update(WorkingHour entity)
        {
            using (var conn = _db.GetConnection())
            {
                var query = @"UPDATE WorkingHours 
                             SET ActiveWorkingHours = @ActiveWorkingHours,
                                 MaintenanceHours = @MaintenanceHours,
                                 Notes = @Notes,
                                 ModifiedBy = @ModifiedBy,
                                 ModifiedDate = @ModifiedDate
                             WHERE WorkingHourId = @WorkingHourId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkingHourId", entity.WorkingHourId);
                    cmd.Parameters.AddWithValue("@ActiveWorkingHours", entity.ActiveWorkingHours);
                    cmd.Parameters.AddWithValue("@MaintenanceHours", entity.MaintenanceHours);
                    cmd.Parameters.AddWithValue("@Notes", (object)entity.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedBy", entity.ModifiedBy);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (var conn = _db.GetConnection())
            {
                var query = "DELETE FROM WorkingHours WHERE WorkingHourId = @WorkingHourId";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WorkingHourId", id);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<VehicleSummary> GetWeeklySummary()
        {
            var summaries = new List<VehicleSummary>();

            using (var conn = _db.GetConnection())
            {
                var query = "SELECT * FROM vw_WeeklyVehicleSummary";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summaries.Add(new VehicleSummary
                            {
                                VehicleId = reader.GetInt32(0),
                                VehicleName = reader.GetString(1),
                                LicensePlate = reader.GetString(2),
                                TotalActiveHours = reader.GetDecimal(3),
                                TotalMaintenanceHours = reader.GetDecimal(4),
                                TotalIdleHours = reader.GetDecimal(5),
                                ActivePercentage = reader.GetDecimal(6),
                                MaintenancePercentage = reader.GetDecimal(7),
                                IdlePercentage = reader.GetDecimal(8),
                                RecordCount = reader.GetInt32(9)
                            });
                        }
                    }
                }
            }

            return summaries;
        }

        public List<GanttDataItem> GetGanttData(List<int> vehicleIds, DateTime startDate, DateTime endDate)
        {
            var data = new List<GanttDataItem>();

            using (var conn = _db.GetConnection())
            {
                var query = @"SELECT v.VehicleName, v.LicensePlate, wh.RecordDate, 
                             wh.ActiveWorkingHours, u.FullName
                             FROM WorkingHours wh
                             INNER JOIN Vehicles v ON wh.VehicleId = v.VehicleId
                             INNER JOIN Users u ON wh.CreatedBy = u.UserId
                             WHERE wh.VehicleId IN (" + string.Join(",", vehicleIds) + @")
                             AND wh.RecordDate BETWEEN @StartDate AND @EndDate
                             ORDER BY v.VehicleName, wh.RecordDate";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new GanttDataItem
                            {
                                VehicleName = reader.GetString(0),
                                LicensePlate = reader.GetString(1),
                                StartDate = reader.GetDateTime(2),
                                EndDate = reader.GetDateTime(2).AddHours((double)reader.GetDecimal(3)),
                                Hours = reader.GetDecimal(3),
                                RecordedBy = reader.GetString(4),
                                Type = "Active"
                            });
                        }
                    }
                }
            }

            return data;
        }

        private WorkingHour MapToWorkingHour(SqlDataReader reader)
        {
            return new WorkingHour
            {
                WorkingHourId = reader.GetInt32(reader.GetOrdinal("WorkingHourId")),
                VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                RecordDate = reader.GetDateTime(reader.GetOrdinal("RecordDate")),
                ActiveWorkingHours = reader.GetDecimal(reader.GetOrdinal("ActiveWorkingHours")),
                MaintenanceHours = reader.GetDecimal(reader.GetOrdinal("MaintenanceHours")),
                IdleHours = reader.GetDecimal(reader.GetOrdinal("IdleHours")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ModifiedBy")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                VehicleName = reader.GetString(reader.GetOrdinal("VehicleName")),
                LicensePlate = reader.GetString(reader.GetOrdinal("LicensePlate")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? "" : reader.GetString(reader.GetOrdinal("CreatedByName")),
                ModifiedByName = reader.IsDBNull(reader.GetOrdinal("ModifiedByName")) ? "" : reader.GetString(reader.GetOrdinal("ModifiedByName"))
            };
        }
    }
}
