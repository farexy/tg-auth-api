namespace TG.Auth.Api.Config.Options
{
    public class AuthJwtTokenSettings : JwtTokenSettings
    {
        public long AccessExpirationTimeSec { get; set; }

        public long RefreshExpirationTimeSec { get; set; }

        public string PrivateKey { get; set; } = default!;
    }
}