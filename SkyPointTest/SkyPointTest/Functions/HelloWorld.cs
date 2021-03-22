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
using SkyPointTest.Interface;
using SkyPointTest.Models;

namespace SkyPointTest.Functions
{
    public class HelloWorld
    {
        private readonly MongoClient _mongoClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;

        private readonly IMongoCollection<Users> userCollection;

        public HelloWorld(MongoClient mongoClient,
            ILogger<CreateUser> logger,
            IConfiguration config,
            ITokenService tokenService)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            var database = _mongoClient.GetDatabase(_config[Settings.Settings.DATABASE_NAME]);
            userCollection = database.GetCollection<Users>(_config[Settings.Settings.COLLECTION_NAME]);
        }

        [FunctionName("HelloWorld")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            IActionResult returnValue = null;
            log.LogInformation("C# HTTP trigger function processed a request.");
           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var postData = JsonConvert.DeserializeObject<UserPostData>(requestBody);




            var exists = userCollection.Find(x => x.Username == postData.Username).Any();

            if (!exists)
            {
                returnValue = new BadRequestObjectResult("Invalid user");
            }
            else
            {
                string tokenUsername = _tokenService.ValidateJwtToken(postData.Token);
                if(postData.Username == tokenUsername)
                    returnValue = new OkObjectResult("Hello World");
            }


            return returnValue;
        }
    }

    public class UserPostData {

        public string Username { get; set; }
        public string Token { get; set; }
    }

}

