using PulumiProvisioner.Aws.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Aws.Builder
{
    public static class PulumiAwsDatabase
    {
        public const string DYNAMO_DB = "DynamoDB";
    }

    internal class PulumiAwsDatabaseBuilder(PulumiAwsOrchestrationBuilder builder) : IPulumiDatabaseBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiDatabaseBuilder AddDocument(string tableName, string primaryKeyIdField)
        {
            builder.AddBuildAction(() =>
            {
                var table = new Document(tableName, primaryKeyIdField);
                return new Dictionary<string, object?>
                {
                    { $"Table:{tableName}-Id", table.Id }
                };
            });
            return this;
        }
    }
}
