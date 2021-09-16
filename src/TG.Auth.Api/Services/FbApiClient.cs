using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TG.Auth.Api.Config.Options;
using TG.Auth.Api.Services.Dto;
using TG.Core.App.Exceptions;

namespace TG.Auth.Api.Services
{
    public class FbApiClient : IFbApiClient
    {
        private static string? _appAccessToken;
        private const string BaseOAuthUri = "https://graph.facebook.com";
        private const string ApiVersion = "v12.0";
        private readonly HttpClient _client;
        private readonly FacebookOptions _facebookOptions;

        public FbApiClient(HttpClient client, IOptionsSnapshot<FacebookOptions> facebookOptions)
        {
            _client = client;
            _facebookOptions = facebookOptions.Value;
        }
        
        public async Task<FbTokenPayload?> GetUserTokenPayloadAsync(string accessToken, CancellationToken cancellationToken)
        {
            var url = BaseOAuthUri + "/debug_token?input_token=" + accessToken + "&access_token=" + await GetAppAccessToken(cancellationToken);
            var response = await _client.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode 
                ? await JsonSerializer.DeserializeAsync<FbTokenPayload>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken)
                : throw new BusinessLogicException("Invalid token");
        }
        
        public async Task<FbUserData> GetUserDataAsync(string id, string accessToken, CancellationToken cancellationToken)
        {
            var url = BaseOAuthUri + $"/{ApiVersion}/{id}?fields=id,first_name,last_name,email&access_token={accessToken}";
            var response = await _client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await JsonSerializer.DeserializeAsync<FbUserData>(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken))!;
        }

        private async Task<string> GetAppAccessToken(CancellationToken cancellationToken)
        {
            if (_appAccessToken != null)
            {
                return _appAccessToken;
            }

            var url = BaseOAuthUri + "/oauth/access_token?grant_type=client_credentials&client_id=" + _facebookOptions.AppId + "&client_secret=" + _facebookOptions.AppSecret;
            var response = await _client.GetAsync(url, cancellationToken);
            var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return _appAccessToken = json.RootElement.GetProperty("access_token")!.GetString()!;
        }
    }
}