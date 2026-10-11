using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> b)
        {
            b.ToTable("Rooms");
            b.HasKey(r => r.Id);

            b.Property(r => r.Name).IsRequired().HasMaxLength(50);
            b.HasIndex(r => r.Name).IsUnique();

            b.Property(r => r.Building).HasMaxLength(50);
            b.Property(r => r.IsActive).HasDefaultValue(true);
        }
    }
}
