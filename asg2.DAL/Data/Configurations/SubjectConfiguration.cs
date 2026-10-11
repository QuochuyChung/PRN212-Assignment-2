using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> b)
        {
            b.ToTable("Subjects");
            b.HasKey(s => s.Id);

            b.Property(s => s.Code).IsRequired().HasMaxLength(20);
            b.HasIndex(s => s.Code).IsUnique();

            b.Property(s => s.Name).IsRequired().HasMaxLength(100);
        }
    }
}
