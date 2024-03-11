using System;
using System.Net.Http;
using System.Threading.Tasks;
using KnightPath.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KnightPath
{
    public static class KnightPathResult
    {
        [FunctionName("knightpathresults")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "knightpath")] HttpRequest req,
            ILogger log)
        {
            string operationId = req.Query["operationId"];
            string host = req.Host.Value;
            DurableTaskResponse durableTaskResponse = null;

            try
            {
                using (HttpClient newClient = new HttpClient())
                {
                    HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("https://{0}/runtime/webhooks/durabletask/instances/{1}", host, operationId));
                    newRequest.Headers.Add("x-functions-key", "MOtU1fSRhQkMAyw5UYmEFQhB5Y85PuxNYhPC0zhXMqCCAzFu6ucoYQ==");
                    //http:///localhost:7071/runtime/webhooks/durabletask/instances/{1}


                    //Read Server Response
                    HttpResponseMessage response = await newClient.SendAsync(newRequest);
                    durableTaskResponse = JsonConvert.DeserializeObject<DurableTaskResponse>(await response.Content.ReadAsStringAsync());
                    if(durableTaskResponse == null)
                        return new ObjectResult(response.StatusCode) {StatusCode = ((int)response.StatusCode)};
                }
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new ObjectResult(StatusCodes.Status500InternalServerError) { StatusCode = StatusCodes.Status500InternalServerError };
            }

            if (durableTaskResponse != null)
            {

                if (durableTaskResponse.runtimeStatus == "Completed")
                    return new OkObjectResult(durableTaskResponse.output);
                else
                    return new ObjectResult(StatusCodes.Status202Accepted) { StatusCode = StatusCodes.Status202Accepted };
            }
            else
                return new ObjectResult(StatusCodes.Status404NotFound) { StatusCode = StatusCodes.Status202Accepted };
        }
    }
}