using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;

namespace PulumiProvisioner.Azure.Templates
{
    public class AppService
    {
        private WindowsWebApp _service = null!;

        internal AppService(string resourceGroupName, string name, string zipDeployPath)
        {
            Create(resourceGroupName, name, zipDeployPath);
        }

        public Output<string> Id => _service.Id;
        public Output<string> Urn => _service.Urn;

        private void Create(string resourceGroupName, string name, string zipDeployPath)
        {
            var plan = new Plan($"{name}-webapp-plan", new PlanArgs
            {
                ResourceGroupName = resourceGroupName,
                Kind = "Windows",
                Sku = new PlanSkuArgs
                {
                    Tier = "Dynamic",
                    Size = "F1"
                }
            });

            _service = new WindowsWebApp(name, new WindowsWebAppArgs
            {
                ServicePlanId = plan.Id,
                ResourceGroupName = resourceGroupName,
                ZipDeployFile = zipDeployPath
            });
        }
    }
}
