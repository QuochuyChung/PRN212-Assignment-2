# Asg2 — Quản lý kỳ thi và lịch thi vấn đáp (Razor Pages + 3 layer + SignalR)

Chia task và hợp đồng chung của nhóm: [asg2_task_split.md](asg2_task_split.md). File này là hướng dẫn chạy và quy ước kỹ thuật.

## 1. Yêu cầu môi trường
- .NET 8 SDK trở lên (project target `net8.0`).
- `dotnet-ef`: `dotnet tool install --global dotnet-ef`.
- Quyền truy cập SQL Server của nhóm: **`192.168.58.24`** (xin user/mật khẩu từ leader).

## 2. Chạy lần đầu

```bash
copy asg2.Web\appsettings.example.json asg2.Web\appsettings.json
```
Mở `asg2.Web/appsettings.json`, thay `YOUR_DB_USER` và `YOUR_DB_PASSWORD` bằng thông tin do leader cấp. **Không commit file này** (đã nằm trong `.gitignore`); connection string chỉ được nằm trong `appsettings.json`.

```bash
dotnet run --project asg2.Web
```
Mở http://localhost:5010 (đổi cổng ở `asg2.Web/Properties/launchSettings.json`).

### Tài khoản mẫu (dữ liệu test do seeder tạo, không dùng cho môi trường thật)
| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin` | `Admin@123` |
| Student | `student1` … `student10` | `Student@123` |

## 3. Database dùng chung: ai được migrate?

DB nằm trên **server chung 192.168.58.24**, nên để không ai làm hỏng lịch sử migration của người khác:

- **Chỉ leader** áp migration và seed lên server chung. Việc áp migration:
  ```bash
  dotnet ef database update --project asg2.DAL --startup-project asg2.Web
  ```
- Seeder chạy khi app khởi động với cờ `Database:InitializeOnStartup = true` (leader bật **một lần**, rồi tắt lại). Có thể không sửa file mà đặt biến môi trường: `set Database__InitializeOnStartup=true`.
- Mặc định cờ này là **`false`**, nên chạy app bình thường không ghi gì lên DB chung.
- Thành viên khác **không** chạy `database update` / bật cờ trên server chung. Muốn thử nghiệm riêng thì dùng DB cục bộ bằng biến môi trường, không đụng file cấu hình:
  ```bash
  set ConnectionStrings__DefaultConnection=Server=(localdb)\MSSQLLocalDB;Database=Asg2Local;Trusted_Connection=True;TrustServerCertificate=True;
  set Database__InitializeOnStartup=true
  dotnet run --project asg2.Web
  ```

### Quy trình migration
1. Tạo migration (chỉ sinh file, không chạm DB): `dotnet ef migrations add <Ten> --project asg2.DAL --startup-project asg2.Web`.
2. Trước khi mở PR có migration: **pull `develop` mới nhất, xóa migration của mình rồi `migrations add` lại** để không đụng `AppDbContextModelSnapshot`.
3. Sau khi PR được merge, leader pull rồi `database update` lên server chung.
4. Hiện migration `InitialCreate` mới chỉ có bảng `Accounts`. Khi các entity khác đã merge, leader có thể gộp lại thành một `InitialCreate` (xóa file migration cũ rồi tạo lại trên DB trống) hoặc tiếp tục thêm migration nối tiếp.

## 4. Kiến trúc 3 lớp

```
asg2.Web   Razor Pages + (ExamHub, SignalRNotifier do P2)    Presentation
   │  chỉ tham chiếu BLL
asg2.BLL   Services, DTO, ServiceResult, IRealtimeNotifier   Business
   │  PrivateAssets="compile"
asg2.DAL   Entities, AppDbContext, Repositories, Migrations  Data
```

### Ràng buộc tuyệt đối: tầng Web không được gọi xuống DAL
Chi tiết ở mục 4.1 của [asg2_task_split.md](asg2_task_split.md). Tóm tắt những gì đã được dựng sẵn:
- `asg2.Web.csproj` **chỉ** tham chiếu BLL. `asg2.BLL.csproj` tham chiếu DAL với `PrivateAssets="compile"`, nên Web gõ `using asg2.DAL` hay dùng EF Core là **lỗi biên dịch**.
- `Program.cs` của Web chỉ gọi hai hàm của BLL: `AddBusinessLayer(configuration)` và `InitializeDatabaseAsync()`.
- Service ở BLL để **`internal`**; chỉ interface và DTO là `public`. Chữ ký `public` của BLL không được chứa entity / DbContext / Repository.
- PageModel chỉ gọi Service (BLL). Cần dữ liệu gì thì thêm method vào Service.
- Chạy **`powershell -NoProfile -File tools/check-layers.ps1`** trước khi mở PR; reviewer chạy lại. Script báo lỗi nếu Web chạm DAL.

### Tự đăng ký DI (không phải sửa `Program.cs`)
- **Repository** trong DAL: class tên `XxxRepository` có interface `IXxxRepository` được tự đăng ký.
- **Service** trong BLL: class tên `XxxService` có interface `IXxxService` được tự đăng ký.
- **Seeder**: class implement `ISeeder` trong `asg2.DAL/Data/Seeding/` tự được chạy theo `Order` (seeder phải idempotent).
- **Entity**: tạo class trong `asg2.DAL/Entities/` và `XxxConfiguration : IEntityTypeConfiguration<Xxx>` trong `asg2.DAL/Data/Configurations/`. Không cần thêm `DbSet` vào `AppDbContext`; repository dùng `db.Set<Xxx>()`.

## 5. Thêm một tính năng mới (lát dọc của mỗi người)

1. **DAL**: entity, configuration, `IXxxRepository` + `XxxRepository` (`internal`), seeder nếu cần.
2. **BLL**: DTO trong `Dtos/`, `IXxxService` (public) + `XxxService` (`internal`), validate trong service, trả `ServiceResult`. Sau khi lưu DB thành công thì gọi `IRealtimeNotifier` (`HubEvents.Xxx` là tên event đã chốt).
3. **Web**: Razor Page trong thư mục của mình; PageModel chỉ bind input → gọi service → xử lý `ServiceResult` (dùng `result.AddToModelState(ModelState, "Input")` để hiện lỗi).
4. **Menu**: thêm đúng một dòng vào `Pages/Shared/_AdminNav.cshtml` hoặc `_StudentNav.cshtml` (chỗ có nhãn tên bạn).
5. **Realtime phía client**: đăng ký handler bằng `ExamEvents.on("TenEvent", fn)`, không tự tạo connection SignalR. Dựng DOM bằng `textContent`, không dùng `innerHTML`. Xem `wwwroot/js/account-list.js` làm mẫu.
6. Có `AppToast.show("nội dung", "success|danger|warning|info")` để báo người dùng.

## 6. Đã có sẵn (P1) và việc kế tiếp

Đã có: khung 3 lớp, đăng nhập bằng cookie + phân quyền `Admin` / `Student`, CRUD tài khoản sinh viên (tìm kiếm, khóa/mở khóa, xóa), tài khoản bị khóa hoặc xóa mất hiệu lực ngay ở request kế tiếp, danh sách tài khoản tự cập nhật khi nhận event, `IRealtimeNotifier` + `HubEvents` (đủ danh sách event theo hợp đồng), toast và layout dùng chung.

**Chờ P2 (lõi SignalR):**
- Thay `wwwroot/js/exam-hub.js` (hiện là stub) bằng kết nối SignalR thật tới `/hubs/exam`, **giữ nguyên API** `ExamEvents.on/off/emit`.
- Viết `ExamHub` (chỉ đẩy xuống client, `[Authorize]`, Admin vào group `admins`) và `SignalRNotifier : IRealtimeNotifier` ở `asg2.Web`, đăng ký trong `Program.cs` để thay `NullRealtimeNotifier`.
- Cho tới khi đó, event realtime chưa tới trình duyệt (server gọi notifier rỗng), nhưng khóa/xóa tài khoản vẫn có hiệu lực ngay ở request kế tiếp.
