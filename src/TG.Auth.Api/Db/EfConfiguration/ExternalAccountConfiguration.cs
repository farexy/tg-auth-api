using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Auth.Api.Entities;

namespace TG.Auth.Api.Db.EfConfiguration
{
    public class ExternalAccountConfiguration : IEntityTypeConfiguration<ExternalAccount>
    {
        public void Configure(EntityTypeBuilder<ExternalAccount> entity)
        {
            entity.HasKey(a => new { a.Id, a.Type });
            entity.HasOne(a => a.TgUser)
                .WithMany()
                .HasForeignKey(a => a.TgUserId);
        }
    }
}