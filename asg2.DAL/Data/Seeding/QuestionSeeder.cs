using asg2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Data.Seeding
{
    /// <summary>
    /// Ngân hàng câu hỏi mẫu cho các môn học (P5).
    /// Chạy khi InitializeOnStartup = true; Order = 40 (sau SubjectRoomSeeder và ExamSessionSeeder).
    /// </summary>
    internal sealed class QuestionSeeder : ISeeder
    {
        public int Order => 40;

        public async Task SeedAsync(AppDbContext db)
        {
            var questions = db.Set<Question>();
            if (await questions.AnyAsync())
            {
                return;
            }

            var subjects = await db.Set<Subject>().ToListAsync();
            var subjectMap = subjects.ToDictionary(s => s.Code, s => s.Id);

            var sampleQuestions = new List<(string SubjectCode, int BloomLevel, string Content)>
            {
                // PRN222 - Lập trình .NET
                ("PRN222", 1, "Phân biệt sự khác nhau giữa kiểu Value Type và Reference Type trong .NET CLR."),
                ("PRN222", 1, "Trình bày vòng đời của một Dependency Injection Service (Transient, Scoped, Singleton) trong ASP.NET Core."),
                ("PRN222", 1, "Cú pháp Linq Method Syntax và Query Syntax khác nhau như thế nào về mặt bản chất?"),
                ("PRN222", 1, "Từ khóa 'yield return' trong C# có tác dụng gì và hoạt động theo cơ chế nào?"),
                ("PRN222", 2, "Giải thích cơ chế hoạt động của middleware pipeline trong ASP.NET Core và ý nghĩa thứ tự đăng ký."),
                ("PRN222", 2, "Trình bày nguyên lý hoạt động của SignalR và các phương thức truyền tin (WebSockets, Server-Sent Events, Long Polling)."),
                ("PRN222", 2, "Entity Framework Core Change Tracker hoạt động như thế nào khi thực hiện câu lệnh SaveChangesAsync?"),
                ("PRN222", 2, "Giải thích sự khác biệt giữa IEnumerable, ICollection và IQueryable trong truy vấn dữ liệu."),
                ("PRN222", 3, "Viết đoạn mã xử lý Concurrency Conflict (DbUpdateConcurrencyException) khi hai người dùng cùng cập nhật một bản ghi."),
                ("PRN222", 3, "Cách thiết kế Custom Middleware trong ASP.NET Core để ghi nhận thời gian thực thi (execution time) của từng request."),
                ("PRN222", 3, "Triển khai kiến trúc 3 lớp với Dependency Injection sao cho tầng Presentation không có quyền truy cập trực tiếp tầng Data Access."),
                ("PRN222", 3, "Áp dụng Fisher-Yates Shuffle để xáo trộn danh sách câu hỏi đảm bảo độ ngẫu nhiên đồng đều O(n)."),
                ("PRN222", 4, "Phân tích và tối ưu hóa hiện tượng N+1 query problem trong Entity Framework Core khi truy vấn dữ liệu quan hệ."),
                ("PRN222", 4, "Thiết kế giải pháp Scale-out cho ứng dụng sử dụng SignalR với Redis Backplane khi số lượng kết nối đồng thời lên tới 10,000."),
                ("PRN222", 4, "So sánh hiệu năng giữa Razor Pages và ASP.NET Core MVC khi xây dựng ứng dụng quản lý dữ liệu hướng trang (page-focused)."),
                ("PRN222", 4, "Đánh giá ưu nhược điểm của cơ chế Optimistic Concurrency so với Pessimistic Concurrency trong hệ thống thi vấn đáp trực tuyến."),

                // SE1234 - Kỹ thuật phần mềm
                ("SE1234", 1, "Định nghĩa các nguyên lý SOLID trong thiết kế hướng đối tượng."),
                ("SE1234", 1, "Trình bày sự khác biệt cơ bản giữa mô hình Waterfall và mô hình Agile/Scrum."),
                ("SE1234", 1, "Nêu vai trò của User Story và tiêu chí nghiệm thu (Acceptance Criteria) trong Scrum."),
                ("SE1234", 2, "Giải thích vai trò của kiến trúc Clean Architecture và luồng phụ thuộc một chiều."),
                ("SE1234", 2, "Phân biệt Unit Test, Integration Test và End-to-End Test."),
                ("SE1234", 2, "Giải thích khái niệm Code Smell và khi nào cần thực hiện Refactoring."),
                ("SE1234", 3, "Áp dụng mẫu thiết kế Repository và Unit of Work để giảm khớp nối (decoupling) trong ứng dụng."),
                ("SE1234", 3, "Xây dựng chiến lược phân nhánh Git (Git Flow) cho một nhóm phát triển phần mềm gồm 5 thành viên."),
                ("SE1234", 3, "Thiết lập pipeline CI/CD cơ bản tự động build và chạy unit tests khi tạo Pull Request."),
                ("SE1234", 4, "Phân tích rủi ro kỹ thuật (Technical Debt) và lập kế hoạch tái cấu trúc một hệ thống mã nguồn cũ."),
                ("SE1234", 4, "Thiết kế hệ thống vi dịch vụ (Microservices) đảm bảo tính toàn vẹn dữ liệu sử dụng mẫu Saga Pattern."),
                ("SE1234", 4, "Xây dựng chiến lược kiểm thử tự động đạt 80% độ phủ mã nguồn (Code Coverage) cho hệ thống tài chính."),

                // IT308 - Cơ sở dữ liệu
                ("IT308", 1, "Định nghĩa các dạng chuẩn 1NF, 2NF, 3NF trong chuẩn hóa cơ sở dữ liệu quan hệ."),
                ("IT308", 1, "Phân biệt Clustered Index và Non-Clustered Index trong SQL Server."),
                ("IT308", 1, "Khóa ngoại (Foreign Key) và quy tắc toàn vẹn tham chiếu (Referential Integrity) có vai trò gì?"),
                ("IT308", 2, "Giải thích tính chất ACID của một Transaction trong hệ quản trị cơ sở dữ liệu."),
                ("IT308", 2, "Phân tích sự khác biệt giữa Inner Join, Left Outer Join và Cross Join."),
                ("IT308", 2, "Thực thi Execution Plan trong SQL Server giúp phát hiện Index Scan thay vì Index Seek như thế nào?"),
                ("IT308", 3, "Viết câu truy vấn tối ưu sử dụng Window Function (ROW_NUMBER(), RANK()) để phân trang kết quả."),
                ("IT308", 3, "Thiết kế chỉ mục kết hợp (Composite Index) để tăng tốc cho câu truy vấn có điều kiện WHERE phức tạp."),
                ("IT308", 3, "Tối ưu hóa trigger và stored procedure trong kịch bản hàng ngàn giao dịch đồng thời."),
                ("IT308", 4, "Phân tích cơ chế khóa (Locking, Deadlock) và giải pháp xử lý Deadlock trong môi trường tải cao."),
                ("IT308", 4, "Chiến lược phân vùng bảng (Table Partitioning) đối với bảng dữ liệu có hàng chục triệu bản ghi."),
                ("IT308", 4, "So sánh cơ sở dữ liệu NoSQL (Document, Key-Value) với RDBMS trong bài toán lưu trữ log và sự kiện realtime."),

                // WD192 - Thiết kế web
                ("WD192", 1, "Trình bày cấu trúc ngữ nghĩa (Semantic Elements) trong HTML5."),
                ("WD192", 1, "Giải thích Box Model trong CSS và sự khác biệt giữa border-box và content-box."),
                ("WD192", 2, "So sánh Flexbox và CSS Grid Layout, khi nào nên sử dụng mỗi loại?"),
                ("WD192", 2, "Event Bubbling và Event Capturing trong DOM JavaScript hoạt động như thế nào?"),
                ("WD192", 3, "Xây dựng giao diện Responsive sử dụng CSS Media Queries và kỹ thuật Mobile-First."),
                ("WD192", 3, "Triển khai xử lý bất đồng bộ trong JavaScript sử dụng Promise và async/await."),
                ("WD192", 4, "Tối ưu hóa Core Web Vitals (LCP, FID, CLS) cho một trang web thương mại điện tử."),
                ("WD192", 4, "Phân tích các lỗ hổng bảo mật phía client phổ biến (XSS, CSRF) và biện pháp phòng chống."),

                // MA101 - Đại số cao cấp
                ("MA101", 1, "Định nghĩa không gian vector và hệ độc lập tuyến tính."),
                ("MA101", 1, "Trình bày cách tính định thức của ma trận vuông cấp n."),
                ("MA101", 2, "Giải thích ý nghĩa hình học của vector riêng và giá trị riêng (Eigenvector, Eigenvalue)."),
                ("MA101", 2, "Định lý Rouché-Capelli về điều kiện có nghiệm của hệ phương trình tuyến tính."),
                ("MA101", 3, "Thực hiện phép chéo hóa ma trận và ứng dụng trong tính lũy thừa ma trận bậc cao."),
                ("MA101", 3, "Ứng dụng phân tích giá trị suy biến (SVD) trong nén ảnh số."),
                ("MA101", 4, "Phân tích thuật toán PageRank của Google dựa trên chuỗi Markov và vector riêng."),

                // MO201 - Phát triển ứng dụng di động
                ("MO201", 1, "Trình bày vòng đời của một Activity trong Android."),
                ("MO201", 1, "Khái niệm State và Stateless Widget trong Flutter."),
                ("MO201", 2, "Giải thích cơ chế quản lý trạng thái (State Management) trong ứng dụng di động."),
                ("MO201", 3, "Tích hợp Push Notification sử dụng Firebase Cloud Messaging (FCM)."),
                ("MO201", 4, "Tối ưu hóa bộ nhớ và thời lượng pin cho ứng dụng định vị thời gian thực.")
            };

            foreach (var item in sampleQuestions)
            {
                if (subjectMap.TryGetValue(item.SubjectCode, out var subjectId))
                {
                    questions.Add(new Question
                    {
                        SubjectId = subjectId,
                        BloomLevel = item.BloomLevel,
                        Content = item.Content,
                        IsActive = true
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
