using System.Text.Json.Serialization;

namespace TG.Auth.Api.Services.Dto
{
    public class FbTokenPayload
    {
        [JsonPropertyName("data")]

        public FbTokenData Data { get; set; } = default!;
    }

    public class FbTokenData
    {
        [JsonPropertyName("app_id")]
        public string AppId { get; set; } = default!;
        
        [JsonPropertyName("expires_at")]
        public int ExpiresAt { get; set; }
        
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = default!;

        [JsonPropertyName("scopes")]
        public string[] Scopes { get; set; } = default!;
    }
}