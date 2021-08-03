using System.Text.Json.Serialization;

namespace TG.Auth.Api.Services.Dto
{
    public class GoogleTokenPayload
    {
        [JsonPropertyName("iss")]
        public string Issuer { get; set; } = default!;

        [JsonPropertyName("aud")]
        public string Audience { get; set; } = default!;
        
        [JsonPropertyName("sub")]
        public string Subject { get; set; } = default!;
        
        [JsonPropertyName("hd")]
        public string HostedDomain { get; set; } = default!;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        
        [JsonPropertyName("picture")]
        public string Picture { get; set; } = default!;
        
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; } = default!;
        
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; } = default!;
    }
}