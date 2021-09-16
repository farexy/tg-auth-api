using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;

namespace TG.Auth.Api.Services
{
    public class LoginGenerator : ILoginGenerator
    {
        private const int MaxLoginLength = 30;

        private readonly ApplicationDbContext _dbContext;
        private readonly ICryptoResistantStringGenerator _stringGenerator;

        public LoginGenerator(ApplicationDbContext dbContext, ICryptoResistantStringGenerator stringGenerator)
        {
            _dbContext = dbContext;
            _stringGenerator = stringGenerator;
        }

        public async Task<string> GenerateLoginAsync(string? email)
        {
            if (email is null)
            {
                return await GenerateTallGuyLoginAsync();
            }

            var emailUsername = email[..email.IndexOf('@')];
            var baseUsername = emailUsername.Length > MaxLoginLength
                ? emailUsername.Substring(0, MaxLoginLength)
                : emailUsername;

            var uniqueUserName = await GenerateUniqueUserName(baseUsername);
            return uniqueUserName;
        }
        
        /// <summary>
        /// Checks user name for duplicate and adds counter for uniqueness if duplicated.
        /// </summary>
        private async Task<string> GenerateUniqueUserName(string baseLogin)
        {
            if (string.IsNullOrWhiteSpace(baseLogin) || baseLogin.Length > MaxLoginLength)
            {
                throw new ArgumentException(@"Invalid length", nameof(baseLogin));
            }
            if (await _dbContext.Users.AllAsync(u => u.Login != baseLogin))
            {
                return baseLogin;
            }

            var userNamesWithSameBeginning = (await _dbContext.Users
                    .Where(u => EF.Functions.Like(u.Login, $"{baseLogin}%"))
                    .Select(u => u.Login)
                    .ToListAsync())
                .ToHashSet();

            var possibleDigitPlaces = MaxLoginLength - baseLogin.Length;
            var maxPossibleNumber = Math.Pow(10, possibleDigitPlaces);
            for (int i = 1; i < maxPossibleNumber; i++)
            {
                var username = baseLogin + i;
                if (!userNamesWithSameBeginning.Contains(username))
                {
                    return username;
                }
            }

            return await GenerateUniqueUserName(baseLogin.Substring(0, baseLogin.Length - 1));
        }

        private async Task<string> GenerateTallGuyLoginAsync()
        {
            const string loginPrefix = "TallGuy_";
            const int postfixLength = 8;
            var generatedLogin = loginPrefix + _stringGenerator.Generate(postfixLength);

            if (await _dbContext.Users.AnyAsync(u => u.Login == generatedLogin))
            {
                return await GenerateTallGuyLoginAsync();
            }

            return generatedLogin;
        }
    }
}