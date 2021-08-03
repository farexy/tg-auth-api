using System;

namespace TG.Auth.Api.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = default!;
        
        public string? GoogleAccountId { get; set; }
    }
}