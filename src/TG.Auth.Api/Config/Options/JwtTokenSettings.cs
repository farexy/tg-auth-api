namespace TG.Auth.Api.Config.Options
{
    // todo move to core
    public class JwtTokenSettings
    {
        public string PublicKey { get; set; } = default!;

        public string Audience { get; set; } = default!;

        public string Issuer { get; set; } = default!;
    }
}