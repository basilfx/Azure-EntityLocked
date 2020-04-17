using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace EntityLocked
{
    public class TimerTrigger
    {
        [FunctionName("TimerTrigger-Activity")]
        public async Task<long> ActivityAsync(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger logger)
        {
            var lastId = context.GetInput<long>();

            // Add some delay to simulate hard work.
            logger.LogInformation("Performing hard work.");

            await Task.Delay(TimeSpan.FromSeconds(5));

            logger.LogInformation("Hard work performed.");

            return lastId + 1;
        }

        [FunctionName("TimerTrigger-Orchestrator")]
        public async Task OrchestratorAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var entity = new EntityId("EntityState", "global");

            using (await context.LockAsync(entity))
            {
                var lastId = await context.CallEntityAsync<long>(entity, "Get");

                var nextLastId = await context.CallActivityAsync<long>("TimerTrigger-Activity", lastId);

                if (lastId != nextLastId)
                {
                    await context.CallEntityAsync(entity, "Set", nextLastId);
                }
            }
        }

        [FunctionName("TimerTrigger")]
        public static Task RunAsync(
            [TimerTrigger("0/10 * * * * *")]TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation("Timer triggerd.");

            return client.StartNewAsync("TimerTrigger-Orchestrator");
        }
    }
}
