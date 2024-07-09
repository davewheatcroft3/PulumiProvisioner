using PulumiProvisioner.Aws.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Aws.Builder
{
    internal class PulumiAwsOrchestrationBuilder : IPulumiOrchestrationBuilder
    {
        private readonly List<Func<Dictionary<string, object?>>> _buildActions = [];

        public IPulumiDatabaseBuilder AddStorage(string name)
        {
            var builder = new PulumiAwsDatabaseBuilder(this);
            return builder;
        }

        public IPulumiOrchestrationBuilder AddEmailSender(string name)
        {
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }

        public IPulumiApplicationBuilder AddWebApi(string apiName, string codeDirectory, string apiProjectName)
        {
            AddBuildAction(() =>
            {
                var lambdaApi = new LambdaApi(apiName, codeDirectory, apiProjectName, apiName, "ANY", true);
                return new Dictionary<string, object?>
                    {
                        { $"Lambda:{apiName}-Id", lambdaApi.LambdaId },
                        { $"ApiGateway:{apiName}-Id", lambdaApi.ApiGatewayId },
                        { $"ApiGateway:{apiName}-Url", lambdaApi.Url }
                    };
            });

            var applicationBuilder = new PulumiAwsApiApplicationBuilder(this);
            return applicationBuilder;
        }

        public IPulumiApplicationBuilder AddBlazorWasm(string appName, string codeDirectory)
        {
            AddBuildAction(() =>
            {
                var website = new StaticWebsite(
                    appName,
                    "index.html",
                    "index.html",
                    codeDirectory,
                    null);
                return new Dictionary<string, object?>
                {
                    { $"S3Bucket:{appName}-Url", website.BucketUrl },
                    { $"Cloudfront:{appName}-Url", website.Url }
                };
            });

            var applicationBuilder = new PulumiAwsWasmApplicationBuilder(this, appName, codeDirectory);
            return applicationBuilder;
        }

        public IPulumiApplicationBuilder AddBlazorServer(string appName, string codeDirectory)
        {
            AddBuildAction(() =>
            {
                var website = new FargateEcsWebsite(
                    appName,
                    codeDirectory);
                return new Dictionary<string, object?>
                {
                    { $"VPC:{appName}-Id", website.VpcId },
                    { $"ApplicationLoadBalancer:{appName}-Id", website.LoadBalancerId },
                    { $"RedisCluster:{appName}-Id", website.RedisClusterId },
                    { $"FargateService:{appName}-Id", website.FargateId }
                };
            });

            var applicationBuilder = new PulumiAwsBlazorServerApplicationBuilder(this);
            return applicationBuilder;
        }

        public IPulumiOrchestrationBuilder AddFireAndForgetComputation(string name, string lambdaCodeDirectory, string projectName)
        {
            AddBuildAction(() =>
            {
                var lambdaWithQueue = new LambdaViaQueue(name, lambdaCodeDirectory, projectName, name);
                return new Dictionary<string, object?>
                {
                    { $"Lambda:{name}-Id", lambdaWithQueue.LambdaId },
                    { $"Sqs:{name}-Id", lambdaWithQueue.QueueId }
                };
            });
            return this;
        }

        public IPulumiOrchestrationBuilder AddComputationWithReturnValue(string name, string lambdaCodeDirectory, string projectName)
        {
            AddBuildAction(() =>
            {
                // This is similar to AddLambda().WithAccessViaApi() except we override the httpmethods and cors settings
                // since we dont want to publicly make these available in the builder...
                var lambdaApi = new LambdaApi(name, lambdaCodeDirectory, projectName, name, "GET", false);
                return new Dictionary<string, object?>
                {
                    { $"Lambda:{name}-Id", lambdaApi.LambdaId },
                    { $"ApiGateway:{name}-Id", lambdaApi.ApiGatewayId },
                    { $"ApiGateway:{name}-Url", lambdaApi.Url }
                };
            });
            return this;
        }

        internal Dictionary<string, object?> Build()
        {
            var outputs = new Dictionary<string, object?>();

            foreach (var action in _buildActions)
            {
                var buildOutputs = action();
                foreach (var buildOutput in buildOutputs)
                {
                    outputs.Add(buildOutput.Key, buildOutput.Value);
                }
            }
            return outputs;
        }

        internal void AddBuildAction(Func<Dictionary<string, object?>> action)
        {
            _buildActions.Add(action);
        }

        internal void RemoveLastBuildAction()
        {
            _buildActions.RemoveAt(_buildActions.Count - 1);
        }
    }
}
