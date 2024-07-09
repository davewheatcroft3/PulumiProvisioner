namespace PulumiProvisioner.Core.Builder
{
    public interface IPulumiDatabaseBuilder
    {
        /// <summary>
        /// Adds a document (table) with a given primary key field.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="primaryKeyIdField">Primary key name that will be your primary key field</param>
        /// <returns>Builder instance</returns>
        IPulumiDatabaseBuilder AddDocument(string tableName, string primaryKeyIdField);

        /// <summary>
        /// Go back to the root builder instance.
        /// </summary>
        IPulumiOrchestrationBuilder Builder { get; }
    }
}
