namespace PulumiProvisioner.Core.Builder
{
    /// <summary>
    /// Builder instance that will create common .NET project types with common needs for simple setups.
    /// As an when your infrastructure gets too complicated, you can easily lift the code to customize further
    /// directly with Pulumi.
    /// </summary>
    public interface IPulumiOrchestrationBuilder
    {
        /// <summary>
        /// Adds a document storage table (AWS Dynamo DB) with a given primary key field.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="primaryKeyIdField">Primary key name that will be your primary key field</param>
        /// <returns>Build instance</returns>
        IPulumiOrchestrationBuilder AddDocumentStorage(string tableName, string primaryKeyIdField);

        IPulumiOrchestrationBuilder AddEmailSender(string name);

        IPulumiApplicationBuilder AddWebApi(string apiName, string codeDirectory, string apiProjectName);

        IPulumiApplicationBuilder AddBlazorWasm(string appName, string codeDirectory);

        IPulumiApplicationBuilder AddBlazorServer(string appName, string codeDirectory);

        IPulumiOrchestrationBuilder AddFireAndForgetComputation(string name, string lambdaCodeDirectory, string projectName);

        IPulumiOrchestrationBuilder AddComputationWithReturnValue(string name, string lambdaCodeDirectory, string projectName);
    }
}
