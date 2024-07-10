namespace PulumiProvisioner.Core.Builder
{
    /// <summary>
    /// Builder instance that will create common .NET project types with common needs for simple setups.
    /// As an when your infrastructure gets too complicated, you can easily lift the code to customize further
    /// directly with Pulumi.
    /// </summary>
    public interface IPulumiOrchestrationBuilder
    {
        IPulumiDatabaseBuilder AddDataStorage(string name);

        IPulumiOrchestrationBuilder AddEmailSender(string name);

        IPulumiOrchestrationBuilder AddFileStorage(string name);

        IPulumiApplicationBuilder AddWebApi(string apiName, string codeDirectory, string apiProjectName);

        IPulumiApplicationBuilder AddBlazorWasm(string appName, string codeDirectory);

        IPulumiApplicationBuilder AddBlazorServer(string appName, string codeDirectory);

        IPulumiOrchestrationBuilder AddFireAndForgetComputation(string name, string lambdaCodeDirectory, string projectName);

        IPulumiOrchestrationBuilder AddComputationWithReturnValue(string name, string lambdaCodeDirectory, string projectName);
    }
}
