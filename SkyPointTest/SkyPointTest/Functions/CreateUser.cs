using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SkyPointTest.Models;
using System;
using SkyPointTest.Settings;

namespace SkyPointTest.Functions
{
    public class CreateUser
    {
        // Reference: https://github.com/willvelida/serverless-mongodb-api
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        private readonly IMongoCollection<Users> userCollection;

        public CreateUser(
            MongoClient mongoClient,
            ILogger<CreateUser> logger,
            IConfiguration config)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;

            var database = _mongoClient.GetDatabase(_config[Settings.Settings.DATABASE_NAME]);
            userCollection = database.GetCollection<Users>(_config[Settings.Settings.COLLECTION_NAME]);
        }

        [FunctionName("SignUp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "SignUp")] HttpRequest req,
            ILogger log)
        {
            IActionResult returnValue = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<Users>(requestBody);

            var user = new Users
            {
               Username = input.Username,
                Password = input.Password,
                
            };

            try
            {
                userCollection.InsertOne(user);
                returnValue = new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


            return returnValue;
        }
    }
}

