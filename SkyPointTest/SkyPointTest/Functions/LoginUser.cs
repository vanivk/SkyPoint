using System;
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
    public class LoginUser
    {

		private readonly MongoClient _mongoClient;
		private readonly ILogger _logger;
		private readonly IConfiguration _config;
		private readonly ITokenService _tokenService;

		private readonly IMongoCollection<Users> userCollection;

        public LoginUser(MongoClient mongoClient,
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

		[FunctionName("SignIn")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SignIn")] HttpRequest req,
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
				var userData = userCollection.Find(x => x.Username == user.Username && x.Password == user.Password).FirstOrDefault();
				if (userData == null)
				{
					returnValue = new NotFoundResult();
				}
				else
				{
					returnValue = new OkObjectResult(new { Username = user.Username, Token = _tokenService.CreateToken(user) });
					
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Could not find user. Exception thrown: {ex.Message}");
				returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
			return returnValue;
		}
    }
}

