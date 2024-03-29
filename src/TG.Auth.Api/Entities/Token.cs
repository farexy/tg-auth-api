using System;
using TG.Auth.Api.Constants;

namespace TG.Auth.Api.Entities
{
    public class Token
    {
        public Guid Id { get; set; }
        
        public Guid UserId { get; set; }

        public string DeviceId { get; set; } = default!;

        public AuthType AuthType { get; set; }
        
        public DateTime IssuedTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public string RefreshSecret { get; set; } = default!;
        
        public User? User { get; set; }
    }
}