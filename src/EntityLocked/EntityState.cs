using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace EntityLocked
{
    public static class EntityState
    {
        [FunctionName("EntityState")]
        public static void Run([EntityTrigger] IDurableEntityContext context)
        {
            // Ensure that a state exists.
            if (!context.HasState)
            {
                context.SetState(0L);
            }

            // Handle operation.
            switch (context.OperationName.ToLowerInvariant())
            {
                case "set":
                    context.SetState(context.GetInput<long>());
                    break;
                case "get":
                    context.Return(context.GetState<long>());
                    break;
                case "delete":
                    context.DeleteState();
                    break;
            }
        }
    }
}
