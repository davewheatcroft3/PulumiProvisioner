using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.DocumentDB.Inputs;
using Pulumi.AzureNative.Resources;

namespace PulumiProvisioner.Azure.Templates
{
    public class Document
    {
        private SqlResourceSqlContainer _table = null!;

        internal Document(string containerName, string tableName, string primaryKeyFieldName)
        {
            Create(containerName, tableName, primaryKeyFieldName);
        }

        public Output<string> Id => _table.Id;

        private void Create(string resourceGroupName, string accountName, string databaseName, string tableName, string idKeyName)
        {
            var resourceGroup = new ResourceGroup(resourceGroupName);

            var container = new DatabaseAccount(accountName, new DatabaseAccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
                Locations =
                {
                    new LocationArgs
                    {
                        LocationName = resourceGroup.Location,
                        FailoverPriority = 0,
                    },
                }
            });

            var database = new SqlResourceSqlDatabase(databaseName, new SqlResourceSqlDatabaseArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = container.Name,
                Resource = new SqlDatabaseResourceArgs
                {
                    Id = idKeyName,
                }
            });

            _table = new SqlResourceSqlContainer(tableName, new SqlResourceSqlContainerArgs
            {
                ResourceGroupName = resourceGroup.Name,
                AccountName = container.Name,
                DatabaseName = database.Name,
                Resource = new SqlContainerResourceArgs
                {
                    Id = tableName,
                    PartitionKey = new ContainerPartitionKeyArgs { Paths = { $"/{idKeyName}" }, Kind = "Hash" },
                },
            });
        }
    }
}
