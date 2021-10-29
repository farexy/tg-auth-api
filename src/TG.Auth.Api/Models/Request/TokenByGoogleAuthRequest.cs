namespace TG.Auth.Api.Models.Request
{
    public class TokenByGoogleAuthRequest
    {
        public string DeviceId { get; set; } = default!;
        public string IdToken { get; set; } = default!;
    }
}