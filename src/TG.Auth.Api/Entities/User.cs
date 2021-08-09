using System;
using TG.Core.App.Constants;

namespace TG.Auth.Api.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = default!;

        public string Email { get; set; } = default!;
        
        public string FirstName { get; set; } = default!;
        
        public string LastName { get; set; } = default!;

        public UserRoles[] Roles { get; set; } = default!;
    }
}