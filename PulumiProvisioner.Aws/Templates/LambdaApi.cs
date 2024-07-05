using Pulumi;
using Pulumi.Aws.ApiGatewayV2;
using Pulumi.Aws.ApiGatewayV2.Inputs;
using Pulumi.Aws.Lambda;

namespace PulumiProvisioner.Aws.Templates
{
    public class LambdaApi : Lambda
    {
        private Api _api = null!;

        internal LambdaApi(
            string lambdaName,
            string lambdaCodeDirectory,
            string handlerName,
            string apiName,
            string? httpMethods = "ANY",
            bool skipCors = true) : base(lambdaName, lambdaCodeDirectory, handlerName)
        {
            CreateApi(apiName, _function, httpMethods ?? "ANY", skipCors);
        }

        public Output<string> ApiGatewayId => _api.Id;

        public Output<string> Url => _api.ApiEndpoint;

        private void CreateApi(string apiName, Function lambda, string httpMethods, bool skipCors)
        {
            _api = skipCors
                ? new Api(apiName, new()
                {
                    ProtocolType = "HTTP",
                    Name = apiName
                })
                : new Api(apiName, new()
                {
                    ProtocolType = "HTTP",
                    Name = apiName,
                    CorsConfiguration = new ApiCorsConfigurationArgs
                    {
                        AllowOrigins = "*",
                        AllowMethods = "*",
                        AllowHeaders = "*"
                    }
                });

            var lambdaIntegration = new Integration($"{apiName}-Lambda-Integration", new IntegrationArgs
            {
                ApiId = _api.Id,
                IntegrationType = "AWS_PROXY",
                IntegrationMethod = "ANY",
                PayloadFormatVersion = "2.0",
                IntegrationUri = lambda.Arn
            });

            var httpApiGatewayRoute = new Route($"{apiName}-DefaultRoute", new RouteArgs
            {
                ApiId = _api.Id,
                RouteKey = $"{httpMethods} /{{proxy+}}",
                Target = lambdaIntegration.Id.Apply(id => $"integrations/{id}")
            });

            var httpApiGatewayStage = new Stage($"{apiName}-Stage", new StageArgs
            {
                ApiId = _api.Id,
                AutoDeploy = true,
                Name = "$default"
            });

            var lambdaPermission = new Permission($"{apiName}-Lambda-Permission", new PermissionArgs
            {
                Action = "lambda:InvokeFunction",
                Function = lambda.Name,
                Principal = "apigateway.amazonaws.com",
                SourceArn = Output.Format($"{_api.ExecutionArn}/*")
            });
        }
    }
}
