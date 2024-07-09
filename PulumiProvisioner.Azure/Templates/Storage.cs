using Pulumi;
using Pulumi.AzureNative.DocumentDB;

namespace PulumiProvisioner.Azure.Templates
{
    public class Storage
    {
        private SqlResourceSqlContainer _table = null!;

        internal Storage(string resourceGroupName, string accountName, string databaseName)
        {
            Create(resourceGroupName, accountName, databaseName);
        }

        public Output<string> Id => _table.Id;

        private void Create(string resourceGroupName, string accountName, string databaseName)
        {
            var container = new DatabaseAccount(accountName, new DatabaseAccountArgs
            {
                ResourceGroupName = resourceGroupName,
                DatabaseAccountOfferType = DatabaseAccountOfferType.Standard
            });

            var database = new SqlResourceSqlDatabase(databaseName, new SqlResourceSqlDatabaseArgs
            {
                ResourceGroupName = resourceGroupName,
                AccountName = container.Name
            });
        }
    }
}
