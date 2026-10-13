using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> b)
        {
            b.ToTable("Questions");
            b.HasKey(q => q.Id);

            b.Property(q => q.Content).IsRequired().HasMaxLength(2000);
            b.Property(q => q.BloomLevel).IsRequired();
            b.Property(q => q.IsActive).IsRequired().HasDefaultValue(true);

            b.HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(q => q.SubjectId);
            b.HasIndex(q => new { q.SubjectId, q.BloomLevel });
        }
    }
}
