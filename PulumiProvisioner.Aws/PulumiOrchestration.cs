using PulumiProvisioner.Aws.Builder;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Aws
{
    public static class PulumiOrchestration
    {
        public static IPulumiOrchestrationBuilder CreateBuilder()
        {
            var builder = new PulumiAwsOrchestrationBuilder();
            return builder;
        }
    }
}