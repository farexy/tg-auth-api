using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Auth.Api.Entities;

namespace TG.Auth.Api.Db.EfConfiguration
{
    public class GoogleAccountConfiguration : IEntityTypeConfiguration<GoogleAccount>
    {
        public void Configure(EntityTypeBuilder<GoogleAccount> entity)
        {
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.TgUser)
                .WithMany()
                .HasForeignKey(a => a.TgUserId);
        }
    }
}