using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Data.Seeding
{
    /// <summary>
    /// Phiên thi mẫu và slot mẫu để dev/test (P3).
    /// Chạy khi InitializeOnStartup = true; Order = 30 (sau AccountSeeder và SubjectRoomSeeder).
    /// </summary>
    internal sealed class ExamSessionSeeder : ISeeder
    {
        public int Order => 30;

        public async Task SeedAsync(AppDbContext db)
        {
            var sessions = db.Set<ExamSession>();
            if (await sessions.AnyAsync())
            {
                return;
            }

            var prn222 = await db.Set<Subject>().FirstOrDefaultAsync(s => s.Code == "PRN222");
            var se1234 = await db.Set<Subject>().FirstOrDefaultAsync(s => s.Code == "SE1234");
            var it308 = await db.Set<Subject>().FirstOrDefaultAsync(s => s.Code == "IT308");

            var roomA101 = await db.Set<Room>().FirstOrDefaultAsync(r => r.Name == "A101");
            var roomB203 = await db.Set<Room>().FirstOrDefaultAsync(r => r.Name == "B203");
            var roomC305 = await db.Set<Room>().FirstOrDefaultAsync(r => r.Name == "C305");

            if (prn222 == null || roomA101 == null)
            {
                return;
            }

            var baseDate = DateTime.UtcNow.Date.AddDays(1); // Ngày mai

            var session1 = new ExamSession
            {
                Title = "Thi vấn đáp PRN222 - Ca sáng",
                SubjectId = prn222.Id,
                RoomId = roomA101.Id,
                StartTime = baseDate.AddHours(8),       // 08:00
                EndTime = baseDate.AddHours(11),        // 11:00 (180 phút -> 12 slots)
                SlotMinutes = 15,
                MainQuestionCount = 2,
                MaxFollowUpCount = 1,
                Status = ExamSessionStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            var session2 = new ExamSession
            {
                Title = "Thi vấn đáp SE1234 - Ca chiều",
                SubjectId = (se1234 ?? prn222).Id,
                RoomId = (roomB203 ?? roomA101).Id,
                StartTime = baseDate.AddHours(13).AddMinutes(30), // 13:30
                EndTime = baseDate.AddHours(16).AddMinutes(30),   // 16:30
                SlotMinutes = 20,
                MainQuestionCount = 2,
                MaxFollowUpCount = 2,
                Status = ExamSessionStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            var session3 = new ExamSession
            {
                Title = "Thi vấn đáp IT308 - Cơ sở dữ liệu",
                SubjectId = (it308 ?? prn222).Id,
                RoomId = (roomC305 ?? roomA101).Id,
                StartTime = baseDate.AddDays(1).AddHours(9),
                EndTime = baseDate.AddDays(1).AddHours(11).AddMinutes(30),
                SlotMinutes = 15,
                MainQuestionCount = 3,
                MaxFollowUpCount = 1,
                Status = ExamSessionStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            sessions.AddRange(session1, session2, session3);
            await db.SaveChangesAsync();

            // Tạo slot mẫu cho student1 và student2 ở session1
            var student1 = await db.Set<Account>().FirstOrDefaultAsync(a => a.Username == "student1");
            var student2 = await db.Set<Account>().FirstOrDefaultAsync(a => a.Username == "student2");

            if (student1 != null)
            {
                var slots = db.Set<ExamSlot>();
                slots.Add(new ExamSlot
                {
                    SessionId = session1.Id,
                    StudentId = student1.Id,
                    OrderNo = 1,
                    StartTime = session1.StartTime,
                    EndTime = session1.StartTime.AddMinutes(session1.SlotMinutes)
                });

                if (student2 != null)
                {
                    slots.Add(new ExamSlot
                    {
                        SessionId = session1.Id,
                        StudentId = student2.Id,
                        OrderNo = 2,
                        StartTime = session1.StartTime.AddMinutes(session1.SlotMinutes),
                        EndTime = session1.StartTime.AddMinutes(session1.SlotMinutes * 2)
                    });
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
