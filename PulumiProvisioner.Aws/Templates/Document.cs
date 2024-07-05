using Pulumi;
using Pulumi.Aws.DynamoDB;
using Pulumi.Aws.DynamoDB.Inputs;

namespace PulumiProvisioner.Aws.Templates
{
    public class Document
    {
        private Table _table = null!;

        internal Document(string name, string primaryKeyFieldName)
        {
            Create(name, primaryKeyFieldName);
        }

        public Output<string> Id => _table.Id;

        private void Create(string tableName, string idKeyName)
        {
            _table = new Table(tableName, new TableArgs
            {
                Name = tableName,
                ReadCapacity = 1,
                WriteCapacity = 1,
                HashKey = idKeyName,
                Attributes =
                {
                    new TableAttributeArgs
                    {
                        Type = "S",
                        Name = idKeyName
                    }
                }
            });

            //table.AutoScaleReadCapacity(new EnableScalingProps { MinCapacity = 1, MaxCapacity = 5 });
            //table.AutoScaleWriteCapacity(new EnableScalingProps { MinCapacity = 1, MaxCapacity = 5 });
        }
    }
}
