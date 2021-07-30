using Microsoft.EntityFrameworkCore;
using TG.Core.Db.Postgres;

namespace TG.Auth.Api.Db
{
    public class ApplicationDbContext : TgDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}