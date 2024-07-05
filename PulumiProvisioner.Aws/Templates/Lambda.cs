using Pulumi;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Iam.Inputs;
using Pulumi.Aws.Lambda;

namespace PulumiProvisioner.Aws.Templates
{
    public class Lambda
    {
        protected Function _function = null!;

        internal Lambda(
            string lambdaName,
            string lambdaCodeDirectory,
            string handlerName)
        {
            CreateLambda(lambdaName, lambdaCodeDirectory, handlerName);
        }

        public Output<string> LambdaId => _function.Id;

        protected virtual List<ManagedPolicy> GetManagedPolicies()
        {
            var logPolicy = ManagedPolicy.CloudWatchFullAccess;
            return [ logPolicy ];
        }

        private void CreateLambda(string lambdaName, string codeFilePath, string handler)
        {
            var role = new Role($"{lambdaName}Role", new RoleArgs
            {
                AssumeRolePolicy = GetPolicyDocument.Invoke(new()
                {
                    Statements = new[]
                    {
                        new GetPolicyDocumentStatementInputArgs
                        {
                            Actions = [ "sts:AssumeRole" ],
                            Effect = "Allow",
                            Principals =
                            [
                                new GetPolicyDocumentStatementPrincipalInputArgs()
                                {
                                    Type = "Service",
                                    Identifiers = [ "lambda.amazonaws.com" ]
                                }
                            ]
                        }
                    }
                }).Apply(x => x.Json),
                ManagedPolicyArns = GetManagedPolicies().Select(x => x.ToString()).ToList()
            });

            _function = new Function($"{lambdaName}", new FunctionArgs
            {
                Name = lambdaName,
                Runtime = Runtime.Dotnet8,
                Code = new FileArchive(codeFilePath),
                Handler = handler,
                Role = role.Arn,
                MemorySize = 512,
                Timeout = 15
            });
        }
    }
}
