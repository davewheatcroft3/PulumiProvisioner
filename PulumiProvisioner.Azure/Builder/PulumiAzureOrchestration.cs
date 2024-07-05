using PulumiProvisioner.Azure.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure.Builder
{
    internal class PulumiAzureOrchestrationBuilder : IPulumiOrchestrationBuilder
    {
        private readonly List<Func<Dictionary<string, object?>>> _buildActions = [];

        public IPulumiOrchestrationBuilder AddDocumentStorage(string tableName, string primaryKeyIdField)
        {
            AddBuildAction(() =>
            {
                var table = new Document("", tableName, primaryKeyIdField);
                return new Dictionary<string, object?>
                {
                    { $"Table:{tableName}-Id", table.Id }
                };
            });
            return this;
        }

        public IPulumiOrchestrationBuilder AddEmailSender(string name)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public IPulumiApplicationBuilder AddWebApi(string apiName, string codeDirectory, string apiProjectName)
        {
            throw new NotImplementedException("Not yet implemented");
        }

        public IPulumiApplicationBuilder AddBlazorWasm(string appName, string codeDirectory)
        {
            var applicationBuilder = new PulumiAzureWasmApplicationBuilder(this, appName, codeDirectory);
            return applicationBuilder;
        }

        public IPulumiApplicationBuilder AddBlazorServer(string appName, string codeDirectory)
        {

            throw new NotImplementedException("Not yet implemented");
        }

        public IPulumiOrchestrationBuilder AddFireAndForgetComputation(string name, string lambdaCodeDirectory, string projectName)
        {

            throw new NotImplementedException("Not yet implemented");
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
