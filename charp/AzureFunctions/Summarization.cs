using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;

namespace OpenAIHackathon
{
    public class Summarization
    {
        private readonly IConfiguration _configuration;

        public Summarization(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("Summarization")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            OpenAIClient client = new OpenAIClient(new Uri(_configuration["openaiuri"]),
                                            new AzureKeyCredential(_configuration["openaikey"]));

            // If streaming is not selected
            Response<Completions> completionsResponse = await client.GetCompletionsAsync(deploymentOrModelName: _configuration["openai-summary-deployment"],
                                                                                            new CompletionsOptions()
                                                                                            {
                                                                                                Prompts = { req.Query["prompt"], },
                                                                                                Temperature = (float)0,
                                                                                                MaxTokens = 1000,
                                                                                                NucleusSamplingFactor = (float)1,
                                                                                                FrequencyPenalty = (float)0,
                                                                                                PresencePenalty = (float)0,
                                                                                                GenerationSampleCount = 1,
                                                                                            });
            Completions completions = completionsResponse.Value;

            string responseMessage = string.IsNullOrEmpty(completions.Choices[0].Text)
                ? "Function to summarise text has been executed successfully but there is no summary, please try again!!!"
                : completions.Choices[0].Text;

            return new OkObjectResult(responseMessage);
        }
    }
}
