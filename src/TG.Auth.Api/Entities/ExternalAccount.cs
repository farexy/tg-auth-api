using System;
using TG.Auth.Api.Constants;

namespace TG.Auth.Api.Entities
{
    public class ExternalAccount
    {
        public string Id { get; set; } = default!;
        
        public AuthType Type { get; set; }

        public string? Email { get; set; }
        
        public Guid TgUserId { get; set; }
        
        public User? TgUser { get; set; }
    }
}