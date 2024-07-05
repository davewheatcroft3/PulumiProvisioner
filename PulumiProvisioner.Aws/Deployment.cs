using PulumiProvisioner.Aws.Builder;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Aws
{
    public static class Deployment
    {
        public static async Task<int> RunAsync(Action<IPulumiOrchestrationBuilder> action)
        {
            var builder = new PulumiAwsOrchestrationBuilder();
            return await Pulumi.Deployment.RunAsync(() =>
            {
                action(builder);
                var outputs = builder.Build();
                return outputs;
            });
        }
    }
}