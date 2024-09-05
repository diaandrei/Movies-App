using Azure.Security.KeyVault.Secrets;

namespace Movies.Identity
{
    public class JwtConfigurationService
    {
        public string TokenSecret { get; }
        public string Issuer { get; }
        public string Audience { get; }

        public JwtConfigurationService(SecretClient secretClient)
        {
            TokenSecret = secretClient.GetSecret("Movies-JWTKey")?.Value?.Value!;
            Issuer = secretClient.GetSecret("Movies-JwtIssuer")?.Value?.Value!;
            Audience = secretClient.GetSecret("Movies-JwtAudience")?.Value?.Value!;
        }
    }
}
