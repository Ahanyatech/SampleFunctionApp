using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;

namespace SampleFunctionApp
{
    public class AccessKeyVault
    {
        private readonly IConfiguration _configuration;
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;
        private readonly IConfigurationRefresher _configurationRefresher;

        public AccessKeyVault(IConfiguration configuration, IFeatureManagerSnapshot featureManagerSnapshot, IConfigurationRefresherProvider refresherProvider)
        {
            _configuration = configuration;
            _featureManagerSnapshot = featureManagerSnapshot;
            _configurationRefresher = refresherProvider.Refreshers.First();
        }

        [FunctionName("AccessKeyVault")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function: AccessKeyVault Execution Started");

            await _configurationRefresher.TryRefreshAsync();

            string message = await _featureManagerSnapshot.IsEnabledAsync("Beta")
                    ? "The Feature Flag 'Beta' is turned ON"
                    : "The Feature Flag 'Beta' is turned OFF";
            string key = "Environment";
            string config = _configuration[key];
            message = message + config;
            return (ActionResult)new OkObjectResult(message);

            //string name = req.Query["key"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "No key has been passed"
            //    : $"Value for the key: {name} has been successfully retrieved and it's value is.";

            //return new OkObjectResult(responseMessage);
        }
    }
}
