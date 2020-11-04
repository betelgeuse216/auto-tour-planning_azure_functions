using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MySqlConnector;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            String logMessage = null;

            var builder = new MySqlConnectionStringBuilder
            {
                Server = "microtourism.mysql.database.azure.com",
                Database = "microtourism",
                UserID = "sai_admin@microtourism",
                Password = "sa1sa!sa1",
                SslMode = MySqlSslMode.Required,
            };

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                Console.WriteLine("Opening connection");
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    //command.CommandText = "UPDATE inventory SET quantity = @quantity WHERE name = @name;";
                    //command.Parameters.AddWithValue("@quantity", 200);
                    //command.Parameters.AddWithValue("@name", "banana");
                    command.CommandText = "SELECT * FROM microtourism.interests";

                    int rowCount = await command.ExecuteNonQueryAsync();
                    MySqlDataReader dataReader = await command.ExecuteReaderAsync();
                    Console.WriteLine(String.Format("Number of rows count={0}", rowCount));

                    while(dataReader.Read())
                    {
                        Console.WriteLine(String.Format("id={0} : name={1}", dataReader.GetValue(0), dataReader.GetValue(1)));
                        logMessage = String.Format("id={0} : name={1}", dataReader.GetValue(0), dataReader.GetValue(1));
                    }
                }

                Console.WriteLine("Closing connection");
            }

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            if (logMessage != null)
            {
                name = logMessage;
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hey you, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
