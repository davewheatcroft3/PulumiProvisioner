using PulumiProvisioner.Azure.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure.Builder
{
    internal class PulumiAzureOrchestrationBuilder(string resourceGroupName) : IPulumiOrchestrationBuilder
    {
        private readonly List<Func<Dictionary<string, object?>>> _buildActions = [];

        public IPulumiDatabaseBuilder AddDataStorage(string name)
        {
            AddBuildAction(() =>
            {
                var storage = new Storage(resourceGroupName, name, name);
                return new Dictionary<string, object?>();
            });

            var builder = new PulumiAzureDatabaseBuilder(this, resourceGroupName, name);
            return builder;
        }

        public IPulumiOrchestrationBuilder AddEmailSender(string name)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public IPulumiOrchestrationBuilder AddFileStorage(string name)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public IPulumiApplicationBuilder AddWebApi(string apiName, string codeDirectory, string apiProjectName)
        {
            AddBuildAction(() =>
            {
                var appService = new AppService(resourceGroupName, apiName, codeDirectory);
                return new Dictionary<string, object?>
                    {
                        { $"AppService:{apiName}-Id", appService.Id },
                        { $"AppService:{apiName}-Urn", appService.Urn }
                    };
            });

            var applicationBuilder = new PulumiAzureApiApplicationBuilder(this);
            return applicationBuilder;
        }

        public IPulumiApplicationBuilder AddBlazorWasm(string appName, string codeDirectory)
        {
            AddBuildAction(() =>
            {
                var appService = new AppService(resourceGroupName, appName, codeDirectory);
                return new Dictionary<string, object?>
                    {
                        { $"AppService:{appName}-Id", appService.Id },
                        { $"AppService:{appName}-Urn", appService.Urn }
                    };
            });

            var applicationBuilder = new PulumiAzureWasmApplicationBuilder(this, appName, codeDirectory);
            return applicationBuilder;
        }

        public IPulumiApplicationBuilder AddBlazorServer(string appName, string codeDirectory)
        {
            AddBuildAction(() =>
            {
                var appService = new AppService(resourceGroupName, appName, codeDirectory);
                return new Dictionary<string, object?>
                    {
                        { $"AppService:{appName}-Id", appService.Id },
                        { $"AppService:{appName}-Urn", appService.Urn }
                    };
            });

            var applicationBuilder = new PulumiAzureBlazorServerApplicationBuilder(this);
            return applicationBuilder;
        }

        public IPulumiOrchestrationBuilder AddFireAndForgetComputation(string name, string lambdaCodeDirectory, string projectName)
        {
            AddBuildAction(() =>
            {
                var functionWithQueue = new QueueWithFunction(resourceGroupName, name, lambdaCodeDirectory);
                return new Dictionary<string, object?>
                {
                    { $"Function:{name}-Id", functionWithQueue.FunctionId },
                    { $"Queue:{name}-Id", functionWithQueue.QueueId }
                };
            });
            return this;
        }

        public IPulumiOrchestrationBuilder AddComputationWithReturnValue(string name, string lambdaCodeDirectory, string projectName)
        {
            throw new NotImplementedException("Not yet implemented");
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
