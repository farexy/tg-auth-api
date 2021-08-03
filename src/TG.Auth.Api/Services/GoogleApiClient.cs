using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TG.Auth.Api.Services.Dto;

namespace TG.Auth.Api.Services
{
    public class GoogleApiClient : IGoogleApiClient
    {
        private static readonly Uri BaseOAuthUri = new Uri("https://oauth2.googleapis.com");
        private readonly HttpClient _client;

        public GoogleApiClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = BaseOAuthUri;
        }

        public async Task<GoogleTokenPayload?> ValidateAndParseTokenAsync(string idToken, CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync("/tokeninfo?id_token=" + idToken, cancellationToken);
            return response.IsSuccessStatusCode 
                ? await JsonSerializer.DeserializeAsync<GoogleTokenPayload>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken)
                : null;
        }
    }
}