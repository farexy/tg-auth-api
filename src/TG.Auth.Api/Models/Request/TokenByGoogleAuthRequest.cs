namespace TG.Auth.Api.Models.Request
{
    public class TokenByGoogleAuthRequest
    {
        public string IdToken { get; set; } = default!;
    }
}