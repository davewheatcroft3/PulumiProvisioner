using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.DocumentDB.Inputs;

namespace PulumiProvisioner.Azure.Templates
{
    public class Document
    {
        private SqlResourceSqlContainer _table = null!;

        internal Document(string resourceGroupName, string accountName, string databaseName, string containerName, string primaryKeyFieldName)
        {
            Create(resourceGroupName, accountName, databaseName, containerName, primaryKeyFieldName);
        }

        public Output<string> Id => _table.Id;

        private void Create(string resourceGroupName, string accountName, string databaseName, string tableName, string idKeyName)
        {
            _table = new SqlResourceSqlContainer(tableName, new SqlResourceSqlContainerArgs
            {
                ResourceGroupName = resourceGroupName,
                AccountName = accountName,
                DatabaseName = databaseName,
                Resource = new SqlContainerResourceArgs
                {
                    Id = tableName,
                    PartitionKey = new ContainerPartitionKeyArgs { Paths = { $"/{idKeyName}" }, Kind = "Hash" },
                },
            });
        }
    }
}
