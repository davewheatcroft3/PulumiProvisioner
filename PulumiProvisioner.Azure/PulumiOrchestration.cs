using PulumiProvisioner.Azure.Builder;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure
{
    public static class PulumiOrchestration
    {
        public static IPulumiOrchestrationBuilder CreateBuilder(string resourceGroupName)
        {
            var builder = new PulumiAzureOrchestrationBuilder(resourceGroupName);
            return builder;
        }
    }
}