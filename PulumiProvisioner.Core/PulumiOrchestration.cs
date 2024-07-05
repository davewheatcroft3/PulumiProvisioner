namespace PulumiProvisioner.Core
{
    public static class PulumiHelper
    {
        private static Pulumi.Config? _config;

        public static string? GetConfig(string key)
        {
            var config = _config ??= new Pulumi.Config();
            return config.Get(key);
        }
    }
}