using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.ServiceBus;

namespace PulumiProvisioner.Azure.Templates
{
    internal class QueueWithFunction
    {
        private Namespace _namespace;
        private Queue _queue;
        private WindowsFunctionApp _function;

        internal QueueWithFunction(string resourceGroupName, string name, string zipDeployPath)
        {
            (_namespace, _queue) = CreateQueue(resourceGroupName, name);

            _function = CreateFunction(resourceGroupName, name, zipDeployPath);
        }

        public Output<string> NamespaceId => _namespace.Id;
        public Output<string> QueueId => _queue.Id;
        public Output<string> NamespaceUrn => _namespace.Id;
        public Output<string> QueueUrn => _queue.Urn;
        public Output<string> FunctionId => _function.Id;

        private (Namespace, Queue) CreateQueue(string resourceGroupName, string queueName)
        {
            var serviceBusNamespace = new Namespace($"{queueName}servicebus", new NamespaceArgs
            {
                Name = queueName,
                Sku = "Basic",
                ResourceGroupName = resourceGroupName
            });

            var queue = new Queue(queueName, new QueueArgs
            {
                Name = queueName,
                NamespaceId = serviceBusNamespace.Id,
            });

            var sharedAccessPolicy = new Pulumi.Azure.EventHub.AuthorizationRule($"{queueName}-sap", new Pulumi.Azure.EventHub.AuthorizationRuleArgs
            {
                Name = $"{queueName}-sap",
                ResourceGroupName = resourceGroupName,
                NamespaceName = serviceBusNamespace.Name,
                EventhubName = queue.Name,
                Send = true,
                Listen = true
            });

            return (serviceBusNamespace, queue);
        }

        private WindowsFunctionApp CreateFunction(string resourceGroupName, string functionName, string zipDeployPath)
        {
            var plan = new Plan($"{functionName}-function-plan", new PlanArgs
            {
                ResourceGroupName = resourceGroupName,
                Kind = "FunctionApp",
                Sku = new PlanSkuArgs
                {
                    Tier = "Dynamic",
                    Size = "Y1"
                }
            });

            var function = new WindowsFunctionApp(functionName, new WindowsFunctionAppArgs
            {
                ResourceGroupName = resourceGroupName,
                ServicePlanId = plan.Id,
                ZipDeployFile = zipDeployPath
            });

            return function;
        }
    }
}
