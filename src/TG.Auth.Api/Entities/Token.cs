using System;

namespace TG.Auth.Api.Entities
{
    public class Token
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public DateTime IssuedTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public string RefreshToken { get; set; } = default!;
    }
}