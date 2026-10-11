using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> b)
        {
            b.ToTable("Accounts");
            b.HasKey(a => a.Id);

            b.Property(a => a.Username).IsRequired().HasMaxLength(50);
            b.HasIndex(a => a.Username).IsUnique();

            b.Property(a => a.PasswordHash).IsRequired().HasMaxLength(500);
            b.Property(a => a.FullName).IsRequired().HasMaxLength(100);

            b.Property(a => a.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            b.Property(a => a.IsActive).HasDefaultValue(true);
        }
    }
}
