# Pulumi and AWS 
Helper code for setting up IaC with C#, Pulumi and AWS.

## Introduction
This repo is for any C# developer that is struggling to get a CI/CD pipeline setup that is either not interested or can't be bothered to learn all the ins and outs of AWS, Github actions, CLI and Pulumi concepts.

Call me lazy - but thats me. The use case is a solo developer that wants to create projects - (monolith, macro or microservices) where within 10 minutes my CI/CD setup, code in a repo, lets build my prototype, and more importantly keep easily pushing changes to prototype.
Without having to worry about obscure infrastructure error messages (i'm looking at you Terraform and Amazon CDK!).

The initial step to create your infrastructure is to use a wrapper around the Pulimu code which providers an easy builder setup to add your infrastructure for the common project types:
WebApi (Lambda with an API gateway)
Static Website (Blazor WASM)
Table (Dynamo DB)

There is also an example for a queue that triggers a lambda, since whilst not a .NET project, is a common use-case.

To cover the use-case I intend to add more (see future plans at the bottom of this readme), but this code is only intended to help get people started. As soon as your project takes off, more people work on it, more load, etc, you will likely need to spend more time on this.
In that scenario, you can simply lift the code from project type template you were using and modify it as your infrastructure gets more complicated (or ideally you are making loads of money and can hire someone more interesting in infrastructure!).

**NOTE: I like Pulumi because it just seems to work (again, I don't like Infrastructure!) - however it costs for a non-solo developer and I'm not sure for more complicated infrastructure setups it fits the bill.**

## Installation
1. Install Nuget package
```
Install-Package PulmumiAwsProvisioner
```

2. Create new project for your IaC, make sure to add these references in your .csproj:
```
<ItemGroup>
  <PackageReference Include="Pulumi" Version="3.60.0" />
  <PackageReference Include="Pulumi.Aws" Version="6.29.0" />
</ItemGroup>
```

And also include MimeTypes if you are using the BlazorWasm/Static website template:
```
<PackageReference Include="MimeTypes" Version="2.0.1">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

3. Add Pulumi.yaml file (example in this repo)

4. In your Program.cs add:
```csharp
using PulumiAwsProvisioner.Builder;

return await Deployment.RunAsync(builder =>
{
    builder.AddDocumentStroage("ExampleTable", "Id");

    builder.AddWebApi(ApplicationName, lambdaCodeDirectory, "ExampleWebApi");
});
```

## Debugging/Running Locally
```console
cd <local path to infrastructure project>\<infrastructure project name>

pulumi stack init <stack name>
pulumi stack select <organization>/<project name>>/<stack name>
pulumi config set --stack <stack name> local "true"
dotnet publish "<path to lambda or web project to publish>" -c Release
pulumi up --config local
```

See the test code for why the config key called 'local' is set, not required if you don't want to use it this way. You can ommit that line and just call 'pulumi up'

Or see the ExampleProgram.cs for all current options.

## CI/CD
Github actions yml file included to help get started. Idea is on commit to the a folder in the repo, trigger a deployment, in this case straight to master.

## Registering a URL and SSL certificate
You can do this anywhere but this assumes AWS Route 53 AND Certificate Manager services have been used.
I didn't do this as part of the code largely because i'm not sure its possible fully but also because to me buying your domain and certificate you do seperately and then your infrastructure links
to it. Maybe the SSL could be part of the code but the domain is paid for so not sure how that would work anyway.

1) Purchase your domain from Route 53
2) This should then mean you have a domain name (once verified) and a hosted zone created for you automatically.
3) Add a certificate in the Certificate Manager (I struggled with automatic verficiation, but via email it worked quickly)
4) Using your domain name, hosted zone name and ssl certificate name you should then be able to use in the builder code and link your applications to your domain.

## Future Plans
### Short term (this would 'complete' the common use case for solo developer wanting something up and running quickly)
1) Intellisense comments for builder
2) Email Sending (SES). Not implemented 'AddEmailSender' method.
3) Blazor server (?) - very complicated because of websockets... but intend to use Fargate and ECS to create AWS infra using the not implemented 'AddWebsite'.'WithEcsAndFargate' method.

### Longer term
1) Option for SNS infront of the SQS
2) AddAuthServer with Cognito (examples of working with Blazor?)
3) ECS example(s)...? Wasm website with ECS...
4) Azure as well?