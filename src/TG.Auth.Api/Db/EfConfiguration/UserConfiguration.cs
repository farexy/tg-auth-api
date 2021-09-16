using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Auth.Api.Entities;

namespace TG.Auth.Api.Db.EfConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        private const string JsonType = "jsonb";

        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Login);
            entity.Property(u => u.Roles)
                .HasColumnType(JsonType);
        }
    }
}