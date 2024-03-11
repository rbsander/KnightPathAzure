using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KnightPath.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace KnightPath
{
    public static class ProcessKnightPath
    {
        private static Dictionary<string, KnightPathResponse> solvedPathDictionary = new Dictionary<string, KnightPathResponse>();

        [FunctionName("processKnightPath")]
        public static async Task<KnightPathResponse> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            SourceTarget sourceTarget = context.GetInput<SourceTarget>();

            KnightPathResponse knightPathResponse = await context.CallActivityAsync<KnightPathResponse>("calculateknightshortestpath", sourceTarget);
            knightPathResponse.operationId = context.InstanceId;

            return knightPathResponse;
        }

        [FunctionName("calculateknightshortestpath")]
        public static KnightPathResponse CalculateKnightShortestPath([ActivityTrigger] SourceTarget sourceTarget, ILogger log)
        {
            Queue<(string currentLocation, string currentPath, int fullPathCount)> pathQueue = new Queue<(string currentLocation, string currentPath, int fullPathCount)>();
            (string currentLocation, string currentPath, int pathCount) path;
            KnightPathResponse previouslySolvedResponse;

            if (solvedPathDictionary.TryGetValue(sourceTarget.source+sourceTarget.target, out previouslySolvedResponse))
                return previouslySolvedResponse;

            pathQueue.Enqueue((sourceTarget.source, sourceTarget.source, 0));

            while(pathQueue.Count > 0)
            {
                path = pathQueue.Dequeue();
                if (!solvedPathDictionary.TryGetValue(sourceTarget.source+path.currentLocation, out previouslySolvedResponse))
                    solvedPathDictionary.Add(sourceTarget.source+path.currentLocation, new KnightPathResponse(sourceTarget.source, path.currentLocation, path.currentPath, path.pathCount, null));

                if (path.currentLocation == sourceTarget.target)
                    return new KnightPathResponse(sourceTarget.source, sourceTarget.target, path.currentPath, path.pathCount, null);

                findNextKnightMoves(path.currentLocation, path.currentPath, path.pathCount);
            }
            log.LogError("No Path Found.");
            return new KnightPathResponse(sourceTarget.source, sourceTarget.target, null, 0, null);

            void findNextKnightMoves(string currentLocation, string currentPath, int pathCount)
            {
                char currentLetter, nextLetter;
                int currentNumber, nextNumber;
                string nextLocation;
                string newPath;
                //There are 8 possible resulting knight moves
                int[,] knightMoves = new int[,] { { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 }, { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 } };

                for (int x = 0; x < knightMoves.GetLength(0); x++)
                {
                    currentLetter = currentLocation[0];
                    currentNumber = (int)Char.GetNumericValue(currentLocation[1]);
                    nextLetter = (char)(Convert.ToInt16(currentLetter) + knightMoves[x, 0]);
                    nextNumber = (currentNumber + knightMoves[x, 1]);

                    if (IsValidChessLocation(nextLetter, nextNumber))//moves off the board will not continue.
                    {
                        nextLocation = nextLetter.ToString() + nextNumber.ToString();
                        if (!currentPath.Contains(nextLocation))//moves that repeat a location will not continue, not the shortest.
                        {
                            newPath = $"{currentPath}:{nextLocation}";
                            pathQueue.Enqueue((nextLocation, newPath, pathCount+1));
                        }
                    }
                }
            }

            bool IsValidChessLocation(char positionLetter, int positionNumber)
            {
                if (!string.IsNullOrEmpty(positionLetter.ToString()) && !string.IsNullOrEmpty(positionNumber.ToString()))
                {
                    if(positionLetter >= 'A' && positionLetter <= 'H' && positionNumber > 0 && positionNumber <= 8)
                    {
                        return true;
                    }
                }
                return false;
            }

        }

        [FunctionName("startknightpathcalculation")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "startknightpathcalculation/{source}/{target}")] 
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string source,
            string target,
            ILogger log)
        {
            SourceTarget sourceTarget = new SourceTarget(source, target);

            string instanceId = await starter.StartNewAsync("processKnightPath", null, sourceTarget);

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            response.Content = new StringContent(instanceId);
            return response;
        }
    }
}