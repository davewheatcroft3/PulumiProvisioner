namespace PulumiProvisioner.Core.Builder
{
    /// <summary>
    /// Allows easy attachment of domain name and ssl certificate to your application being built.
    /// </summary>
    public interface IPulumiApplicationBuilder
    {
        /// <summary>
        /// Associate a domain name with your compatible application (WebAPI, Blazor WASM, Blazor Server).
        /// </summary>
        /// <param name="domainName">The domain name without subdomain e.g. google.com</param>
        /// <param name="subDomain">
        /// The subdomain of your fully qualified domain name e.g. www.google.com.
        /// Will default to www if not specified.
        /// </param>
        /// <param name="sslDomainName">
        /// The domain name used to find the SSL certificate defined in AWS Route 53 and certificate manager.
        /// Will default to the fully qualified domain name if not specified.
        /// </param>
        /// <returns></returns>
        IPulumiOrchestrationBuilder WithDomainName(string domainName, string subDomain = "www", string? sslDomainName = null);

        /// <summary>
        /// Go back to the root builder instance.
        /// </summary>
        IPulumiOrchestrationBuilder Builder { get; }
    }
}
