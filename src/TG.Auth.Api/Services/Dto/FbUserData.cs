using System.Text.Json.Serialization;

namespace TG.Auth.Api.Services.Dto
{
    public class FbUserData
    {
        [JsonPropertyName("id")] public string Id { get; set; } = default!;

        [JsonPropertyName("first_name")] public string? FirstName { get; set; }
        
        [JsonPropertyName("last_name")] public string? LastName { get; set; }

        [JsonPropertyName("email")] public string? Email { get; set; }
    }
}