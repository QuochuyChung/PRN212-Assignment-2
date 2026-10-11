using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Data.Seeding
{
    /// <summary>
    /// Môn học và phòng thi mẫu để dev/test (KHÔNG dùng cho môi trường thật).
    /// Chạy khi InitializeOnStartup = true; idempotent — bảng đã có dữ liệu thì bỏ qua.
    /// </summary>
    internal sealed class SubjectRoomSeeder : ISeeder
    {
        // Order 20: chạy sau AccountSeeder (10); P3 (phiên thi) sẽ có Order > 20 để tham chiếu được.
        public int Order => 20;

        private static readonly (string Code, string Name)[] Subjects =
        {
            ("PRN222", "Lập trình .NET"),
            ("SE1234", "Kỹ thuật phần mềm"),
            ("MA101", "Đại số cao cấp"),
            ("IT308", "Cơ sở dữ liệu"),
            ("WD192", "Thiết kế web"),
            ("MO201", "Phát triển ứng dụng di động")
        };

        private static readonly (string Name, string? Building)[] Rooms =
        {
            ("A101", "Tòa A"),
            ("A102", "Tòa A"),
            ("B203", "Tòa B"),
            ("B204", "Tòa B"),
            ("C305", "Tòa C")
        };

        public async Task SeedAsync(AppDbContext db)
        {
            var subjects = db.Set<Subject>();
            if (!await subjects.AnyAsync())
            {
                foreach (var (code, name) in Subjects)
                {
                    subjects.Add(new Subject { Code = code, Name = name });
                }
            }

            var rooms = db.Set<Room>();
            if (!await rooms.AnyAsync())
            {
                foreach (var (name, building) in Rooms)
                {
                    rooms.Add(new Room { Name = name, Building = building, IsActive = true });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
