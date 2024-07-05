using PulumiProvisioner.Aws.Templates;
using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Aws.Builder
{
    internal class PulumiAwsApiApplicationBuilder(PulumiAwsOrchestrationBuilder builder) : IPulumiApplicationBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null)
        {
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }
    }

    internal class PulumiAwsWasmApplicationBuilder(
            PulumiAwsOrchestrationBuilder builder,
            string appName,
            string codeDirectory) : IPulumiApplicationBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null)
        {
            builder.RemoveLastBuildAction();
            builder.AddBuildAction(() =>
            {
                var website = new StaticWebsite(
                    appName,
                    "index.html",
                    "index.html",
                    codeDirectory,
                    new DomainArgs(domainName, subDomain, sslDomainName));
                return new Dictionary<string, object?>
                {
                    { $"S3Bucket:{appName}-Url", website.BucketUrl },
                    { $"Cloudfront:{appName}-Url", website.Url }
                };
            });

            return builder;
        }
    }

    internal class PulumiAwsBlazorServerApplicationBuilder : IPulumiApplicationBuilder
    {
        private readonly PulumiAwsOrchestrationBuilder _builder;

        public PulumiAwsBlazorServerApplicationBuilder(PulumiAwsOrchestrationBuilder builder)
        {
            _builder = builder;
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }

        public IPulumiOrchestrationBuilder Builder => _builder;

        public IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null)
        {
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }
    }
}
