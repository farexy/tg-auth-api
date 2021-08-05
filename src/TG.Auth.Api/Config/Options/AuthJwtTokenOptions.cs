using TG.Core.App.Configuration.Auth;

namespace TG.Auth.Api.Config.Options
{
    public class AuthJwtTokenOptions : JwtTokenOptions
    {
        public long AccessExpirationTimeSec { get; set; }

        public long RefreshExpirationTimeMin { get; set; }

        public string PrivateKey { get; set; } = default!;
        
        public string Secret { get; set; } = default!;
    }
}