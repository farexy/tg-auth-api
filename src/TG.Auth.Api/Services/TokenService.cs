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
        private readonly ICryptoResistantStringGenerator _cryptoResistantStringGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOptionsSnapshot<AuthJwtTokenOptions> _jwtTokenSettings;
        private readonly ApplicationDbContext _dbContext;

        public TokenService(ICryptoResistantStringGenerator cryptoResistantStringGenerator, IDateTimeProvider dateTimeProvider,
            IOptionsSnapshot<AuthJwtTokenOptions> jwtTokenSettings, ApplicationDbContext dbContext)
        {
            _cryptoResistantStringGenerator = cryptoResistantStringGenerator;
            _dateTimeProvider = dateTimeProvider;
            _jwtTokenSettings = jwtTokenSettings;
            _dbContext = dbContext;
        }
        
        public async Task<TokensResponse> CreateTokenAsync(User user, AuthType authType, CancellationToken cancellationToken)
        {
            var tokenId = Guid.NewGuid();

            var token = new Token
            {
                Id = tokenId,
                UserId = user.Id,
                ExpirationTime = RefreshTokenExpirationTime,
                AuthType = authType,
                IssuedTime = _dateTimeProvider.UtcNow,
                RefreshSecret = _cryptoResistantStringGenerator.Generate(RefreshSecretLength),
            };
            
            var accessToken = GenerateAccessToken(user, authType);
            var refreshToken = GenerateRefreshToken(token);

            await _dbContext.AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new TokensResponse(accessToken, refreshToken);
        }

        public async Task<OperationResult<TokensResponse>> ValidateAndRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var payload = tokenHandler.ValidateToken(refreshToken, ValidationParameters, out _);
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
                
                var accessToken = GenerateAccessToken(token.User!, token.AuthType);
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

        private string GenerateAccessToken(User user, AuthType authType)
        {
            var settings = _jwtTokenSettings.Value;
            var accessTokenPayload = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = settings.Issuer,
                [JwtRegisteredClaimNames.Aud] = settings.Audience,
                [JwtRegisteredClaimNames.Exp] = _dateTimeProvider.UtcNow.AddSeconds(settings.AccessExpirationTimeSec).ToUnixTime(),
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Amr] = authType.ToString(),
                [TgClaimNames.Roles] = user.Roles.Select(r => r.ToString()),
            };

            var credentials = new SigningCredentials(settings.PrivateKey.AsRsaPrivateKey(), SecurityAlgorithms.RsaSha512);
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
            _dateTimeProvider.UtcNow.AddSeconds(_jwtTokenSettings.Value.RefreshExpirationTimeSec);
        
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