using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace YourNamespace
{
    [ApiController]
    [Route("[controller]")]
    public class CounterController : ControllerBase
    {
        private readonly string _connectionString = "Server=tcp:shimon-server.database.windows.net,1433;Initial Catalog=shimon-db;Persist Security Info=False;User ID=shimon;Password=kMT0K871[DC^;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [HttpGet("inc")]
        public async Task<IActionResult> Increment()
        {
            string id = "shim";
            var counter = await GetCounterByIdAsync(id);
            if (counter == null)
            {
                return NotFound();
            }
            counter.Value++;
            await UpsertCounterAsync(counter);
            return Ok(counter);
        }

        [HttpGet("dec")]
        public async Task<IActionResult> Decrement()
        {
            string id = "shim";
            var counter = await GetCounterByIdAsync(id);
            if (counter == null)
            {
                return NotFound();
            }
            counter.Value--;
            await UpsertCounterAsync(counter);
            return Ok(counter);
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetValue()
        {
            string id = "shim";
            var counter = await GetCounterByIdAsync(id);
            if (counter == null)
            {
                return NotFound();
            }
            return Ok(counter);
        }

        private async Task<Counter> GetCounterByIdAsync(string id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Id, Value FROM Counters WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Counter
                            {
                                Id = reader.GetString(0),
                                Value = reader.GetInt32(1)
                            };
                        }
                    }
                }
                // If not found, create a new Counter
                var newCounter = new Counter
                {
                    Id = id,
                    Value = 0
                };
                await UpsertCounterAsync(newCounter);
                return newCounter;
            }
        }

        private async Task UpsertCounterAsync(Counter counter)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                IF EXISTS (SELECT 1 FROM Counters WHERE Id = @Id)
                BEGIN
                    UPDATE Counters SET Value = @Value WHERE Id = @Id
                END
                ELSE
                BEGIN
                    INSERT INTO Counters (Id, Value) VALUES (@Id, @Value)
                END";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", counter.Id);
                    command.Parameters.AddWithValue("@Value", counter.Value);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

    public class Counter
    {
        public string Id { get; set; }
        public int Value { get; set; }
    }
}