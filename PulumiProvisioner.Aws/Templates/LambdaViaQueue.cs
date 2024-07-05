using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Sqs;

namespace PulumiProvisioner.Aws.Templates
{
    public class LambdaViaQueue : Lambda
    {
        private Queue _queue = null!;

        internal LambdaViaQueue(
            string lambdaName,
            string lambdaCodeDirectory,
            string handlerName,
            string queueName) : base(lambdaName, lambdaCodeDirectory, handlerName)
        {
            CreateQueue(queueName, lambdaName, _function);
        }

        public Output<string> QueueId => _queue.Id;

        protected override List<ManagedPolicy> GetManagedPolicies()
        {
            var logPolicy = ManagedPolicy.CloudWatchFullAccess;
            var sqsPolicy = ManagedPolicy.AmazonSQSFullAccess;
            return [ logPolicy, sqsPolicy ];
        }

        private void CreateQueue(string queueName, string lambdaName, Function lambda)
        {
            _queue = new Queue(queueName, new()
            {
                Name = queueName
            });

            _ = new EventSourceMapping($"{queueName}-{lambdaName}-EventSource", new()
            {
                FunctionName = lambda.Name,
                EventSourceArn = _queue.Arn
            });
        }
    }
}
