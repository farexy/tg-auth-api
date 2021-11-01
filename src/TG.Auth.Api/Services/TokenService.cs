using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TG.Auth.Api.Config.Options;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Extensions;
using TG.Auth.Api.Models.Response;
using TG.Core.App.Constants;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;

namespace TG.Auth.Api.Services
{
    public class TokenService : ITokenService
    {
        private const int RefreshSecretLength = 32;
        private static readonly JwtSecurityTokenHandler TokenHandler;
        private readonly ICryptoResistantStringGenerator _cryptoResistantStringGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOptionsSnapshot<AuthJwtTokenOptions> _jwtTokenSettings;
        private readonly ApplicationDbContext _dbContext;
        private readonly IRsaParser _rsaParser;

        static TokenService()
        {
            TokenHandler = new JwtSecurityTokenHandler {MapInboundClaims = false};
        }
        
        public TokenService(ICryptoResistantStringGenerator cryptoResistantStringGenerator, IDateTimeProvider dateTimeProvider,
            IOptionsSnapshot<AuthJwtTokenOptions> jwtTokenSettings, ApplicationDbContext dbContext, IRsaParser rsaParser)
        {
            _cryptoResistantStringGenerator = cryptoResistantStringGenerator;
            _dateTimeProvider = dateTimeProvider;
            _jwtTokenSettings = jwtTokenSettings;
            _dbContext = dbContext;
            _rsaParser = rsaParser;
        }
        
        public async Task<TokensResponse> CreateTokenAsync(User user, string deviceId, AuthType authType, CancellationToken cancellationToken)
        {
            var token = await _dbContext.Tokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id && t.DeviceId == deviceId, cancellationToken);

            if (token is null)
            {
                token = new Token
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DeviceId = deviceId,
                };
                await _dbContext.AddAsync(token, cancellationToken);
            }
            
            token.ExpirationTime = RefreshTokenExpirationTime;
            token.AuthType = authType;
            token.IssuedTime = _dateTimeProvider.UtcNow;
            token.RefreshSecret = _cryptoResistantStringGenerator.Generate(RefreshSecretLength);
            
            var accessToken = GenerateAccessToken(user, deviceId, authType);
            var refreshToken = GenerateRefreshToken(token);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new TokensResponse(accessToken, refreshToken);
        }

        public async Task<OperationResult<TokensResponse>> ValidateAndRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            try
            {
                var payload = TokenHandler.ValidateToken(refreshToken, ValidationParameters, out _);
                var tokenId = Guid.Parse(payload.FindFirstValue(JwtRegisteredClaimNames.Jti));
                var refreshSecret = payload.FindFirstValue(JwtRegisteredClaimNames.Acr);

                var token = await _dbContext.Tokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == tokenId, cancellationToken);
                if (token is null || !token.RefreshSecret.SequenceEqual(refreshSecret))
                {
                    return ErrorResult.Create("Refresh token validation failed");
                }

                token.ExpirationTime = RefreshTokenExpirationTime;
                token.IssuedTime = _dateTimeProvider.UtcNow;
                token.RefreshSecret = _cryptoResistantStringGenerator.Generate(RefreshSecretLength);
                
                var accessToken = GenerateAccessToken(token.User!, token.DeviceId, token.AuthType);
                var newRefreshToken = GenerateRefreshToken(token);

                await _dbContext.SaveChangesAsync(cancellationToken);
                
                return new TokensResponse(accessToken, newRefreshToken);
            }
            catch (SecurityTokenException)
            {
                return ErrorResult.Create("Refresh token validation failed");
            }
            catch (ArgumentException)
            {
                return ErrorResult.Create("Invalid token format");
            }
        }

        private string GenerateAccessToken(User user, string deviceId, AuthType authType)
        {
            var settings = _jwtTokenSettings.Value;
            var accessTokenPayload = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = settings.Issuer,
                [JwtRegisteredClaimNames.Aud] = settings.Audience,
                [JwtRegisteredClaimNames.Exp] = _dateTimeProvider.UtcNow.AddSeconds(settings.AccessExpirationTimeSec).ToUnixTime(),
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = user.Login,
                [JwtRegisteredClaimNames.Sid] = deviceId,
                [JwtRegisteredClaimNames.Amr] = authType.ToString(),
                [TgClaimNames.Roles] = user.Roles.Select(r => r.ToString()),
            };

            if (authType is AuthType.GoogleAdmin)
            {
                accessTokenPayload.AddClaim(new Claim(JwtRegisteredClaimNames.Email, user.Email!));
            }

            var rsaPrivateKey = _rsaParser.ParseRsaPrivateKey(settings.PrivateKey);
            var credentials = new SigningCredentials(rsaPrivateKey, SecurityAlgorithms.RsaSha512);
            var header = new JwtHeader(credentials);
            var jwtToken = new JwtSecurityToken(header, accessTokenPayload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwtToken);
        }

        private string GenerateRefreshToken(Token token)
        {
            var settings = _jwtTokenSettings.Value;
            var accessTokenPayload = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = settings.Issuer,
                [JwtRegisteredClaimNames.Aud] = settings.Audience,
                [JwtRegisteredClaimNames.Exp] = token.ExpirationTime.ToUnixTime(),
                [JwtRegisteredClaimNames.Sub] = token.UserId.ToString(),
                [JwtRegisteredClaimNames.Jti] = token.Id.ToString(),
                [JwtRegisteredClaimNames.Acr] = token.RefreshSecret,
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(credentials);
            var jwtToken = new JwtSecurityToken(header, accessTokenPayload);
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(jwtToken);
        }

        private DateTime RefreshTokenExpirationTime =>
            _dateTimeProvider.UtcNow.AddMinutes(_jwtTokenSettings.Value.RefreshExpirationTimeMin);
        
        private TokenValidationParameters ValidationParameters
        {
            get
            {
                var settings = _jwtTokenSettings.Value;

                var parameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret!)),
                    ClockSkew = TimeSpan.Zero,
                };
                return parameters;
            }
        }
    }
}