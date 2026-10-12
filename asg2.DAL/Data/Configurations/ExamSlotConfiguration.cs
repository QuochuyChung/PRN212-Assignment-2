using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class ExamSlotConfiguration : IEntityTypeConfiguration<ExamSlot>
    {
        public void Configure(EntityTypeBuilder<ExamSlot> b)
        {
            b.ToTable("ExamSlots");
            b.HasKey(s => s.Id);

            b.Property(s => s.OrderNo).IsRequired();
            b.Property(s => s.StartTime).IsRequired();
            b.Property(s => s.EndTime).IsRequired();

            b.HasOne(s => s.Session)
                .WithMany(session => session.Slots)
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(s => new { s.SessionId, s.OrderNo });
            b.HasIndex(s => new { s.StudentId, s.StartTime, s.EndTime });
        }
    }
}
