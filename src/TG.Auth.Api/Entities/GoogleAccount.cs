using System;

namespace TG.Auth.Api.Entities
{
    public class GoogleAccount
    {
        public string Id { get; set; } = default!;

        public string Email { get; set; } = default!;
        
        public Guid TgUserId { get; set; }
        
        public User? TgUser { get; set; }
    }
}