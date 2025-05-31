using LiquidLabs.Dto;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace LiquidLabs.Services
{
    public class SystemUnitService : ISystemUnitService
    {
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly ConfigurationSettings _configurationSettings;

        public SystemUnitService(IOptions<ConnectionStringsOptions> options, IOptions<ConfigurationSettings> configurationSettings,
            HttpClient httpClient)
        {
            _connectionString = options.Value.MasterConnection;
            _httpClient = httpClient;
            _configurationSettings = configurationSettings.Value;
            EnsureDatabaseAndTable();
        }

        private void EnsureDatabaseAndTable()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var targetDb = _configurationSettings.TargetDatabase;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var createDbCmd = new SqlCommand($@"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{targetDb}')
                    BEGIN CREATE DATABASE [{targetDb}] END", connection);
                createDbCmd.ExecuteNonQuery();
            }

            var targetConnStr = _connectionString.Replace(_configurationSettings.MasterDatabase, targetDb);
            using (var connection = new SqlConnection(targetConnStr))
            {
                connection.Open();

                var createTableCmd = new SqlCommand(@"IF OBJECT_ID(N'SystemUnits', N'U') IS NULL
                BEGIN CREATE TABLE SystemUnits (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        ExternalId NVARCHAR(50),
                        Name NVARCHAR(200),
                        CPUModel NVARCHAR(100),
                        HardDiskSize NVARCHAR(100),
                        Year INT,
                        Price FLOAT) END", connection);
                createTableCmd.ExecuteNonQuery();
            }
        }

        public async Task<List<SystemUnit>> GetAllSystemUnitsAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_configurationSettings.RestApiUrl);
                var jsonArray = JArray.Parse(response);

                var list = new List<SystemUnit>();
                var dbConnStr = _connectionString.Replace(_configurationSettings.MasterDatabase, _configurationSettings.TargetDatabase);

                using var connection = new SqlConnection(dbConnStr);
                connection.Open();

                foreach (var item in jsonArray)
                {
                    var data = item["data"] as JObject;
                    if (data == null || !data.Properties().Any())
                    {
                        continue;
                    }

                    var unit = new SystemUnit
                    {
                        ExternalId = item["id"]?.ToString(),
                        Name = item["name"]?.ToString(),
                        CPUModel = data["CPU model"]?.ToString(),
                        HardDiskSize = data["Hard disk size"]?.ToString(),
                        Year = (int?)data["year"] ?? 0,
                        Price = (double?)data["price"] ?? 0
                    };

                    var checkCmd = new SqlCommand("SELECT COUNT(*) FROM SystemUnits WHERE ExternalId = @ExternalId", connection);
                    checkCmd.Parameters.AddWithValue("@ExternalId", unit.ExternalId);
                    var exists = (int)checkCmd.ExecuteScalar() > 0;
                    if (exists) 
                    {
                        list.Add(unit);
                        continue; 
                    }

                    var insertCmd = new SqlCommand(@"INSERT INTO SystemUnits (ExternalId, Name, CPUModel, HardDiskSize, Year, Price)
                        VALUES (@ExternalId, @Name, @CPUModel, @HardDiskSize, @Year, @Price)", connection);

                    insertCmd.Parameters.AddWithValue("@ExternalId", unit.ExternalId ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Name", unit.Name ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@CPUModel", unit.CPUModel ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@HardDiskSize", unit.HardDiskSize ?? (object)DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Year", unit.Year);
                    insertCmd.Parameters.AddWithValue("@Price", unit.Price);

                    insertCmd.ExecuteNonQuery();
                    list.Add(unit);
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error occurred while fetching or saving System units", ex);
            }
        }

        public async Task<SystemUnit?> GetSystemUnitByIdAsync(int externalId)
        {
            var dbConnStr = _connectionString.Replace(_configurationSettings.MasterDatabase, _configurationSettings.TargetDatabase);

            await using var connection = new SqlConnection(dbConnStr);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("SELECT Id, ExternalId, Name, CPUModel, HardDiskSize, Year, Price FROM SystemUnits WHERE ExternalId = @ExternalId",
                connection);

            cmd.Parameters.AddWithValue("@ExternalId", externalId.ToString());

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new SystemUnit
                {
                    ExternalId = reader["ExternalId"] as string,
                    Name = reader["Name"] as string,
                    CPUModel = reader["CPUModel"] as string,
                    HardDiskSize = reader["HardDiskSize"] as string,
                    Year = reader["Year"] != DBNull.Value ? (int)reader["Year"] : 0,
                    Price = reader["Price"] != DBNull.Value ? (double)reader["Price"] : 0
                };
            }
            return null;
        }
    }
}
