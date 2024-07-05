using PulumiProvisioner.Azure.Builder;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure
{
    public static class PulumiOrchestration
    {
        public static IPulumiOrchestrationBuilder CreateBuilder()
        {
            var builder = new PulumiAzureOrchestrationBuilder();
            return builder;
        }
    }
}