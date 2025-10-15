using System.Configuration;
using System.Data.SqlClient;

namespace VehicleRentalManagement.DataAccess
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["VehicleRentalDB"].ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
