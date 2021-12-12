using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TG.Auth.Api.Entities;

namespace TG.Auth.Api.Db.EfConfiguration
{
    public class BanConfiguration : IEntityTypeConfiguration<Ban>
    {
        public void Configure(EntityTypeBuilder<Ban> entity)
        {
            entity.HasKey(b => b.Id);
            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId);
        }
    }
}