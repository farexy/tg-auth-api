using System;
using TG.Core.App.Constants;

namespace TG.Auth.Api.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = default!;

        public string? Email { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public UserRoles[] Roles { get; set; } = default!;
        
        public Guid? BanId { get; set; }
    }
}