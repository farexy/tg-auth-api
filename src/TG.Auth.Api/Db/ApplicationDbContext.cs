using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Entities;
using TG.Core.Db.Postgres;

namespace TG.Auth.Api.Db
{
    public class ApplicationDbContext : TgDbContext
    {
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Token> Tokens { get; set; } = default!;
        public DbSet<ExternalAccount> ExternalAccounts { get; set; } = default!;
        
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}