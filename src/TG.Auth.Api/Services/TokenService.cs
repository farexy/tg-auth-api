using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TG.Auth.Api.Config.Options;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Extensions;
using TG.Core.App.Constants;
using TG.Core.App.Services;

namespace TG.Auth.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly ICryptoResistantStringGenerator _generator;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOptionsSnapshot<AuthJwtTokenSettings> _jwtTokenSettings;

        public TokenService(ICryptoResistantStringGenerator generator, IDateTimeProvider dateTimeProvider, IOptionsSnapshot<AuthJwtTokenSettings> jwtTokenSettings)
        {
            _generator = generator;
            _dateTimeProvider = dateTimeProvider;
            _jwtTokenSettings = jwtTokenSettings;
        }

        public string GenerateTokenPair(User user)
        {
            var settings = _jwtTokenSettings.Value;
            var accessTokenPayload = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = settings.Issuer,
                [JwtRegisteredClaimNames.Aud] = settings.Audience,
                [JwtRegisteredClaimNames.Exp] = _dateTimeProvider.UtcNow.AddSeconds(settings.AccessExpirationTimeSec).ToUnixTime(),
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [TgClaimNames.Roles] = user.Roles.Select(r => r.ToString()),
            };

            var credentials = new SigningCredentials(settings.PrivateKey.AsRsaPrivateKey(), SecurityAlgorithms.RsaSha512);
            var header = new JwtHeader(credentials);
            var jwtToken = new JwtSecurityToken(header, accessTokenPayload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwtToken);
        }
    }
}