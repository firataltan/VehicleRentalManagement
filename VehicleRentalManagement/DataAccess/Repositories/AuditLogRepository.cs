using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using VehicleRentalManagement.Models;

namespace VehicleRentalManagement.DataAccess.Repositories
{
    public class AuditLogRepository
    {
        private readonly string _connectionString;

        public AuditLogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Audit log kaydı ekler
        /// </summary>
        public void LogAction(string tableName, int recordId, string action, string oldValues, string newValues, int changedBy)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                        INSERT INTO AuditLogs (TableName, RecordId, Action, OldValue, NewValue, UserId, LogDate)
                        VALUES (@TableName, @RecordId, @Action, @OldValue, @NewValue, @UserId, GETDATE())", connection))
                    {
                        command.Parameters.AddWithValue("@TableName", tableName);
                        command.Parameters.AddWithValue("@RecordId", recordId);
                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@OldValue", (object)oldValues ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NewValue", (object)newValues ?? DBNull.Value);
                        command.Parameters.AddWithValue("@UserId", changedBy);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log hatası sessizce yutulur - ana işlemi etkilememeli
                Console.WriteLine($"Audit log hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tüm audit logları getirir (Admin için)
        /// </summary>
        public List<AuditLog> GetAll(int? limitCount = null, string tableName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var logs = new List<AuditLog>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT TOP (@LimitCount)
                        al.LogId,
                        al.TableName,
                        al.RecordId,
                        al.Action,
                        al.OldValue,
                        al.NewValue,
                        al.UserId,
                        al.LogDate,
                        u.Username,
                        u.FullName
                    FROM AuditLogs al
                    INNER JOIN Users u ON al.UserId = u.UserId
                    WHERE 1=1";

                // Dinamik filtreler
                if (!string.IsNullOrEmpty(tableName))
                {
                    query += " AND al.TableName = @TableName";
                }

                if (startDate.HasValue)
                {
                    query += " AND al.LogDate >= @StartDate";
                }

                if (endDate.HasValue)
                {
                    query += " AND al.LogDate <= @EndDate";
                }

                query += " ORDER BY al.LogDate DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LimitCount", limitCount ?? 1000);
                    
                    if (!string.IsNullOrEmpty(tableName))
                        command.Parameters.AddWithValue("@TableName", tableName);
                    
                    if (startDate.HasValue)
                        command.Parameters.AddWithValue("@StartDate", startDate.Value);
                    
                    if (endDate.HasValue)
                        command.Parameters.AddWithValue("@EndDate", endDate.Value.AddDays(1).AddSeconds(-1));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AuditLog
                            {
                                LogId = reader.GetInt32(reader.GetOrdinal("LogId")),
                                TableName = reader.GetString(reader.GetOrdinal("TableName")),
                                RecordId = reader.GetInt32(reader.GetOrdinal("RecordId")),
                                Action = reader.GetString(reader.GetOrdinal("Action")),
                                OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
                                NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                LogDate = reader.GetDateTime(reader.GetOrdinal("LogDate")),
                                UserName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return logs;
        }

        /// <summary>
        /// Belirli bir tablo ve kayıt için audit logları getirir
        /// </summary>
        public List<AuditLog> GetByRecord(string tableName, int recordId)
        {
            var logs = new List<AuditLog>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"
                    SELECT 
                        al.LogId,
                        al.TableName,
                        al.RecordId,
                        al.Action,
                        al.OldValue,
                        al.NewValue,
                        al.UserId,
                        al.LogDate,
                        u.FullName
                    FROM AuditLogs al
                    INNER JOIN Users u ON al.UserId = u.UserId
                    WHERE al.TableName = @TableName AND al.RecordId = @RecordId
                    ORDER BY al.LogDate DESC", connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@RecordId", recordId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new AuditLog
                            {
                                LogId = reader.GetInt32(reader.GetOrdinal("LogId")),
                                TableName = reader.GetString(reader.GetOrdinal("TableName")),
                                RecordId = reader.GetInt32(reader.GetOrdinal("RecordId")),
                                Action = reader.GetString(reader.GetOrdinal("Action")),
                                OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
                                NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                LogDate = reader.GetDateTime(reader.GetOrdinal("LogDate")),
                                UserName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return logs;
        }

        /// <summary>
        /// İstatistik bilgileri getirir
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"
                    SELECT 
                        Action,
                        COUNT(*) as Count
                    FROM AuditLogs
                    WHERE LogDate >= DATEADD(DAY, -30, GETDATE())
                    GROUP BY Action", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats[reader.GetString(0)] = reader.GetInt32(1);
                        }
                    }
                }
            }

            return stats;
        }
    }
}