using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace VehicleRentalManagement.DataAccess
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        // IConfiguration dependency injection
        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("VehicleRentalDB");
        }

        public string ConnectionString => _connectionString;

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
