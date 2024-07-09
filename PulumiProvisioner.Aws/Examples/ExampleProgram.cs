namespace PulumiProvisioner.Aws.Examples;

public class ExampleProgram
{
    const string ApplicationName = "Example";
    // NOTE: from infrastructure folder, path to your web project
    const string fileCopyDirectory = "../Web";
    // NOTE: this is the path on your builder server where the compiled lambda is (see github actions file)
    const string lambdaCodeDirectory = "./lambda";

    public static async Task<int> ExampleCode()
    {
        return await Deployment.RunAsync(builder =>
        {
            builder
                .AddStorage("Database")
                .AddDocument("ExampleTable", "Id");

            builder.AddEmailSender(ApplicationName);

            builder.AddWebApi(ApplicationName, lambdaCodeDirectory, "ExampleWebApi");

            builder
                .AddBlazorWasm(ApplicationName, fileCopyDirectory)
                .WithDomainName("www.hello.com");

            builder.AddBlazorServer(ApplicationName, fileCopyDirectory);

            builder.AddFireAndForgetComputation(ApplicationName, lambdaCodeDirectory, "ExampleLambdaProject");

            builder.AddComputationWithReturnValue(ApplicationName, lambdaCodeDirectory, "ExampleLambdaProject");
        });
    }

    public static async Task<int> ExampleCodeWithPulumiDeployment()
    {
        var builder = PulumiOrchestration.CreateBuilder();

        return await Pulumi.Deployment.RunAsync(() =>
        {
            // new PulimiAwsProvisioner.Templates.Table(...);
            builder
                .AddStorage("Database")
                .AddDocument("MyTable", "Id");

            // TODO: Email sender template here
            builder.AddEmailSender(ApplicationName);

            // new PulimiAwsProvisioner.Templates.LambdaApi(...);
            builder.AddWebApi(ApplicationName, lambdaCodeDirectory, "ExampleWebApi");

            // new PulimiAwsProvisioner.Templates.StaticWebsite(...);
            builder
                .AddBlazorWasm(ApplicationName, fileCopyDirectory)
                .WithDomainName("www.hello.com");

            // new PulimiAwsProvisioner.Templates.FargateEcsWebsite(...);
            builder.AddBlazorServer(ApplicationName, fileCopyDirectory);

            // new PulimiAwsProvisioner.Templates.LambdaApi(...);
            builder.AddFireAndForgetComputation(ApplicationName, lambdaCodeDirectory, "ExampleLambdaProject");

            // new PulimiAwsProvisioner.Templates.LambdaApi(...);
            builder.AddComputationWithReturnValue(ApplicationName, lambdaCodeDirectory, "ExampleLambdaProject");
        });
    }
}