using PulumiProvisioner.Core.Builder;

namespace PulumiProvisioner.Azure.Builder
{
    internal class PulumiAzureApiApplicationBuilder(PulumiAzureOrchestrationBuilder builder) : IPulumiApplicationBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null)
        {
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }
    }

    internal class PulumiAzureWasmApplicationBuilder(
            PulumiAzureOrchestrationBuilder builder,
            string appName,
            string codeDirectory) : IPulumiApplicationBuilder
    {
        public IPulumiOrchestrationBuilder Builder => builder;

        public IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null)
        {
            throw new NotImplementedException("Not yet implemented - but plan to - see Readme");
        }
    }

    internal class PulumiAzureBlazorServerApplicationBuilder : IPulumiApplicationBuilder
    {
        private readonly PulumiAzureOrchestrationBuilder _builder;

        public PulumiAzureBlazorServerApplicationBuilder(PulumiAzureOrchestrationBuilder builder)
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
