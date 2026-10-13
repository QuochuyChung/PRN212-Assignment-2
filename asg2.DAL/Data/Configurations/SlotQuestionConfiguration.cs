using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace asg2.DAL.Data.Configurations
{
    internal sealed class SlotQuestionConfiguration : IEntityTypeConfiguration<SlotQuestion>
    {
        public void Configure(EntityTypeBuilder<SlotQuestion> b)
        {
            b.ToTable("SlotQuestions");
            b.HasKey(sq => new { sq.SlotId, sq.QuestionId });

            b.Property(sq => sq.OrderNo).IsRequired();

            b.HasOne(sq => sq.Slot)
                .WithMany(s => s.SlotQuestions)
                .HasForeignKey(sq => sq.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(sq => sq.Question)
                .WithMany(q => q.SlotQuestions)
                .HasForeignKey(sq => sq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(sq => new { sq.SlotId, sq.OrderNo });
        }
    }
}
