using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Entities;
using TG.Core.Db.Postgres;

namespace TG.Auth.Api.Db
{
    public class ApplicationDbContext : TgDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<GoogleAccount> GoogleAccounts { get; set; }
        
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}