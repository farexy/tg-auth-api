namespace TG.Auth.Api.Models.Request
{
    public class TokenByFacebookAuthRequest
    {
        public string DeviceId { get; set; } = default!;
        public string AccessToken { get; set; } = default!;
    }
}