namespace TG.Auth.Api.Models.Response
{
    public class TokensResponse
    {
        public TokensResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; }
        
        public string RefreshToken { get; }
    }
}