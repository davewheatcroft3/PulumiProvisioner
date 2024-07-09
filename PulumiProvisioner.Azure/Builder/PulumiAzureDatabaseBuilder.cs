using PulumiProvisioner.Azure.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure.Builder
{
    internal class PulumiAzureDatabaseBuilder(PulumiAzureOrchestrationBuilder builder, string resourceGroupName, string name)  : IPulumiDatabaseBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiDatabaseBuilder AddDocument(string tableName, string primaryKeyIdField)
        {
            builder.AddBuildAction(() =>
            {
                var table = new Document(resourceGroupName, name, name, tableName, primaryKeyIdField);
                return new Dictionary<string, object?>
                {
                    { $"Table:{tableName}-Id", table.Id }
                };
            });
            return this;
        }
    }
}
