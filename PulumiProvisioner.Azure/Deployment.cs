using PulumiProvisioner.Azure.Builder;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure
{
    public static class Deployment
    {
        public static async Task<int> RunAsync(string resourceGroupName, Action<IPulumiOrchestrationBuilder> action)
        {
            var builder = new PulumiAzureOrchestrationBuilder(resourceGroupName);
            return await Pulumi.Deployment.RunAsync(() =>
            {
                action(builder);
                var outputs = builder.Build();
                return outputs;
            });
        }
    }
}