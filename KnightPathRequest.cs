using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace KnightPath
{
    public static class KnightPathRequest
    {
        [FunctionName("computeknightpath")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "knightpath")] 
            HttpRequest req,
            ILogger log)
        {
            string source = req.Query["source"];
            string target = req.Query["target"];
            source = source.ToUpper();
            target = target.ToUpper();

            bool validSource = IsValidChessLocation(source);
            bool validTarget = IsValidChessLocation(target);
            string opertationId = String.Empty;
            string host = req.Host.Value;

            if (validSource && validTarget)
            {
                try
                {
                    using (HttpClient newClient = new HttpClient())
                    {
                        HttpRequestMessage newRequest = new HttpRequestMessage(HttpMethod.Post, string.Format("https://{0}/api/startknightpathcalculation/{1}/{2}", host, source, target));
                        //http:///localhost:7071/api/startknightpathcalculation/{0}/{1}

                        HttpResponseMessage response = await newClient.SendAsync(newRequest);
                        opertationId = await response.Content.ReadAsStringAsync();
                    }

                }
                catch (Exception e)
                {
                    log.LogError(e.Message);
                    return new ObjectResult(StatusCodes.Status500InternalServerError) { StatusCode = StatusCodes.Status500InternalServerError };
                }
            }

            string responseMessage = validSource && validTarget && !string.IsNullOrEmpty(opertationId)
                ? opertationId
                : "No operation was created. A valid source and target must be provided.";

            return new OkObjectResult(responseMessage);

            bool IsValidChessLocation(string position)
            {
                if (!string.IsNullOrEmpty(position) && position.Length == 2)
                {
                    if (Regex.IsMatch(position[0].ToString(), @"^[A-H]+$") && Regex.IsMatch(position[1].ToString(), @"^[1-8]+$"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }    
}