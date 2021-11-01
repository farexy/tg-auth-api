namespace TG.Auth.Api.Models.Request
{
    public class TokenByAppleAuthRequest
    {
        public string DeviceId { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}