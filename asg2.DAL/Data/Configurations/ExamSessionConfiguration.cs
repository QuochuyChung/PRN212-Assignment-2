using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
    {
        public void Configure(EntityTypeBuilder<ExamSession> b)
        {
            b.ToTable("ExamSessions");
            b.HasKey(s => s.Id);

            b.Property(s => s.Title).IsRequired().HasMaxLength(150);

            b.Property(s => s.StartTime).IsRequired();
            b.Property(s => s.EndTime).IsRequired();

            b.Property(s => s.SlotMinutes).HasDefaultValue(15);
            b.Property(s => s.MainQuestionCount).HasDefaultValue(2);
            b.Property(s => s.MaxFollowUpCount).HasDefaultValue(1);

            b.Property(s => s.Status).HasConversion<int>().IsRequired();
            b.Property(s => s.CreatedAt).IsRequired();

            b.HasOne(s => s.Subject)
                .WithMany()
                .HasForeignKey(s => s.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(s => s.Room)
                .WithMany()
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(s => s.SubjectId);
            b.HasIndex(s => s.RoomId);
            b.HasIndex(s => new { s.RoomId, s.StartTime, s.EndTime });
        }
    }
}
