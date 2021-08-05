using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace TG.Auth.Api.Extensions
{
    public static class StringExtensions
    {
        public static RsaSecurityKey AsRsaPrivateKey(this string privateKey)
        {
            using var reader = new StringReader(privateKey);
            var pemReader = new PemReader(reader);
            var assymetricKeyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            if (assymetricKeyPair is null)
            {
                throw new ArgumentException("Invalid key", nameof(privateKey));
            }

            var privateKeyParams = assymetricKeyPair.Private as RsaPrivateCrtKeyParameters;

            var rsaParameters = DotNetUtilities.ToRSAParameters(privateKeyParams);
            return new RsaSecurityKey(RSA.Create(rsaParameters));
        }
    }
}