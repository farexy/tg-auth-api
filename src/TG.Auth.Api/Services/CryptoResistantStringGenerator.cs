using System;
using System.Security.Cryptography;

namespace TG.Auth.Api.Services
{
    public class CryptoResistantStringGenerator : ICryptoResistantStringGenerator
    {
        private const int MinLength = 3;
        private const int MaxLength = 100;
        private const int BitCountCoeff = 6;
        private const int BitInBytes = 8;

        public string Generate(int length)
        {
            if (length < MinLength || length > MaxLength)
            {
                throw new ArgumentException("Invalid length", nameof(length));
            }

            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var bitCount = length * BitCountCoeff;
            var byteCount = (bitCount + BitInBytes - 1) / BitInBytes;
            var bytes = new byte[byteCount];
            rngCryptoServiceProvider.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("/", string.Empty, StringComparison.Ordinal);
        }
    }
}