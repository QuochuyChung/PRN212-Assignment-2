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

Seeder cũng tạo dữ liệu mẫu P2: 6 môn (`PRN222`, `SE1234`, `MA101`, `IT308`, `WD192`, `MO201`) và 5 phòng (`A101`, `A102`, `Tòa A` … `C305`, `Tòa C`).

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

## 6. Đã có sẵn và việc kế tiếp

**P1 ✅:** khung 3 lớp, đăng nhập cookie + phân quyền `Admin` / `Student`, CRUD tài khoản sinh viên (tìm kiếm, khóa/mở khóa, xóa), tài khoản bị khóa hoặc xóa mất hiệu lực ngay ở request kế tiếp, toast và layout dùng chung.

**P2 ✅ (lõi SignalR + môn/phòng/dashboard):**
- `ExamHub` (`/hubs/exam`, `[Authorize]`, admin vào group `admins`) và `SignalRNotifier` thay `NullRealtimeNotifier` trong `Program.cs`.
- `wwwroot/js/exam-hub.js` là kết nối SignalR thật tới `/hubs/exam`, **giữ nguyên API** `ExamEvents.on/off/emit`.
- `Admin/Subjects`, `Admin/Rooms` (CRUD + tìm kiếm), `Admin/Dashboard` (handler `?handler=Stats` trả JSON).
- `subject-list.js`, `room-list.js`, `dashboard.js`: danh sách và số liệu cập nhật live — chi tiết mục 7.

**P3 ✅:** phiên thi, lịch thi chung và trang lịch thi cá nhân của sinh viên.

**P4 ✅:** gán nhiều sinh viên vào phiên, tự chia slot, chặn trùng giờ/vượt sức chứa và cập nhật realtime cho admin + sinh viên.

**P5 ✅ (ngân hàng câu hỏi và bốc câu hỏi):**
- Entities & Repositories: `Question`, `SlotQuestion`, `IQuestionRepository`, `ISlotQuestionRepository`.
- Services: `QuestionService` (CRUD, validate nội dung không rỗng, mức Bloom 1-4, chống trùng nội dung trong môn, chặn xóa câu hỏi đã gán), `QuestionPickerService` (bốc ngẫu nhiên, thuật toán tránh trùng slot liền kề).
- Pages: `Admin/Questions` (Index, Create, Edit, ToggleActive), `Student/SlotDetail` (chi tiết câu hỏi của slot).
- Assignments tích hợp: tự động bốc đề khi gán sinh viên; bổ sung nút "Bốc lại tất cả câu hỏi" và "Bốc lại đề" cho từng slot ở `Admin/Assignments`.
- Realtime: `QuestionCreated/Updated/Deleted` (admin), `SlotQuestionsUpdated` (sinh viên và admin). Khi admin bốc lại đề, trang `SlotDetail` của sinh viên cập nhật câu hỏi ngay lập tức.
- Seeder: `QuestionSeeder` tạo sẵn 10-15 câu hỏi đa dạng mức Bloom cho các môn.
- Kiểm thử tự động: `tools/test-question-picker` kiểm tra 10/10 ca test thành công 100%.

## 7. Realtime (SignalR)

### Luồng một sự kiện

```
PageModel (validate hợp lệ)
  → Service (BLL): lưu DB THÀNH CÔNG trước
  → IRealtimeNotifier  (Service chỉ biết interface này)
  → SignalRNotifier (Web) → IHubContext<ExamHub>
  → /hubs/exam  (gửi tới group / user / tất cả)
  → exam-hub.js: connection.on → ExamEvents.emit("TenEvent", payload)
  → trang: ExamEvents.on("TenEvent", handler) cập nhật UI
```

Service chỉ bắn event **sau khi** lưu DB thành công; lỗi nghiệp vụ trả `ServiceResult`, không bắn gì cả.

### Quy tắc phía client

- `wwwroot/js/exam-hub.js` giữ **MỘT connection duy nhất** cho cả ứng dụng; **trang KHÔNG tự tạo** kết nối SignalR mới — chỉ đăng ký `ExamEvents.on("TenEvent", fn)`.
- Payload JSON do SignalR serialize dạng **camelCase**; dựng DOM bằng `textContent`, không `innerHTML`.
- Sau khi rớt rồi nối lại, hub phát pseudo-event `ExamHubReconnected` để trang tự tải lại dữ liệu (danh sách/`?handler=Stats`).

### Danh sách event (đúng `HubEvents`)

| Event | Bắn từ | Người nhận | Payload |
|---|---|---|---|
| `AccountCreated` / `AccountUpdated` | Admin/Accounts | nhóm `admins` | `AccountDto` |
| `AccountDeleted` | Admin/Accounts | nhóm `admins` | `int` (id) |
| `ForceLogout` | Admin khóa/xóa tài khoản | chính user đó | `null` |
| `SubjectCreated` / `SubjectUpdated` | Admin/Subjects | nhóm `admins` | `SubjectDto` |
| `SubjectDeleted` | Admin/Subjects | nhóm `admins` | `int` (id) |
| `RoomCreated` / `RoomUpdated` | Admin/Rooms | nhóm `admins` | `RoomDto` |
| `RoomDeleted` | Admin/Rooms | nhóm `admins` | `int` (id) |
| `SessionCreated` / `SessionUpdated` / `SessionDeleted` | Admin/Sessions (P3) | mọi client | `SessionDto` / `int` (id) |
| `SlotAssigned` / `SlotRemoved` | Admin/Assignments (P4) | user đó + `admins` | `SlotDto` / `int` (id) |
| `QuestionCreated` / `QuestionUpdated` / `QuestionDeleted` | Admin/Questions (P5) | nhóm `admins` | `QuestionDto` / `int` (id) |
| `SlotQuestionsUpdated` | bốc câu (P5) | user đó + `admins` | `{ slotId, sessionId, questions }` |

### Tự kiểm realtime (2 cửa sổ)

1. Cửa sổ A mở Dashboard, cửa sổ B thêm/xóa một môn hoặc phòng → danh sách và số liệu ở cửa sổ A đổi ngay.
2. Mở `/hubs/exam` ở cửa sổ ẩn danh → bị 401 (hub có `[Authorize]`).
3. DevTools → Network → WS: payload đúng bảng trên (DTO cho Created/Updated, id trần cho Deleted).

## 8. Gán sinh viên và tự chia slot (P4)

Admin mở **Gán sinh viên**, chọn một phiên thi sắp tới rồi tick một hoặc nhiều sinh viên. Hệ thống:

- tính sức chứa bằng `floor((EndTime - StartTime) / SlotMinutes)`;
- ưu tiên slot trống có thứ tự nhỏ nhất và tự tính giờ bắt đầu/kết thúc;
- chặn gán trùng trong cùng phiên, vượt sức chứa, tài khoản không hợp lệ và lịch thi bị chồng giờ;
- giữ lại vị trí slot khi gỡ sinh viên để lần gán sau lấp đúng chỗ trống sớm nhất;
- cập nhật trực tiếp trang admin qua `SlotAssigned` / `SlotRemoved`, đồng thời chỉ gửi thay đổi lịch cá nhân tới đúng sinh viên;
- tính lại slot khi giờ hoặc thời lượng phiên thay đổi; từ chối thay đổi nếu sức chứa mới không đủ hoặc làm sinh viên trùng lịch.

Sinh viên xem kết quả tại **Lịch của tôi**. Khi admin gán, gỡ hoặc đổi giờ phiên, bảng lịch cá nhân cập nhật qua SignalR mà không cần tải lại trang.

## 9. Ngân hàng câu hỏi và thuật toán bốc đề (P5)

### Thuật toán bốc đề ngẫu nhiên tránh trùng slot liền trước
1. Khi bốc câu hỏi cho slot thứ $n$ trong phiên:
   - Hệ thống xác định danh sách các câu hỏi đã được gán cho slot thứ $n-1$ (slot liền trước).
   - Ngân hàng câu hỏi của môn được chia làm 2 tập: $A$ (các câu chưa xuất hiện ở slot $n-1$) và $B$ (các câu đã xuất hiện ở slot $n-1$).
   - Cả 2 tập được xáo trộn ngẫu nhiên bằng thuật toán Fisher-Yates shuffle.
2. Nếu ngân hàng câu hỏi còn đủ ($|A| \ge \text{MainQuestionCount}$):
   - Chọn đủ số câu hoàn toàn từ tập $A$. Đảm bảo **0% trùng lặp** giữa 2 thí sinh liền nhau.
3. Nếu ngân hàng thiếu câu hỏi ($|A| < \text{MainQuestionCount}$):
   - Lấy toàn bộ tập $A$, sau đó bù số lượng còn thiếu từ tập $B$. Đảm bảo độ trùng lặp là **tối thiểu**.

### Kiểm thử tự động
Chạy bộ kiểm thử tự động tại thư mục gốc:
```bash
dotnet run --project tools/test-question-picker
```
Kiểm tra 10 ca kiểm thử bao gồm: validation rỗng, mức Bloom 1..4, chống trùng nội dung trong môn, 50 lượt chạy độc lập chứng minh 0% trùng lặp liên tiếp, cơ chế bù trùng tối thiểu khi thiếu câu hỏi, bốc đồng loạt cả phiên và chống xóa câu hỏi đã dùng.

## 10. Kịch bản Demo 7 phút (Toàn luồng)

1. **P1 (Auth & Phân quyền)**: Đăng nhập Admin (Cửa sổ 1) và Student1 (Cửa sổ 2 ẩn danh). Chứng minh student1 không thể truy cập các đường dẫn `/Admin/*`.
2. **P2 (Môn/Phòng & Dashboard)**: Mở Dashboard ở Cửa sổ 1; Cửa sổ 1 thêm một phòng thi mới -> số liệu thống kê Dashboard và danh sách phòng tự nhảy số realtime.
3. **P3 (Phiên thi & Lịch chung)**: Admin tạo phiên thi mới cho môn PRN222 (phòng A101, 2 câu hỏi/slot) -> Sinh viên ở Cửa sổ 2 thấy phiên thi hiện lên tức thì trên trang "Lịch thi chung".
4. **P4 (Gán sinh viên & Chia slot)**: Admin mở "Gán sinh viên", chọn phiên thi, tick gán `student1` và `student2` -> Hệ thống chia Slot #1 và Slot #2, "Lịch của tôi" của `student1` hiện slot kèm giờ thi ngay.
5. **P5 (Ngân hàng câu hỏi & Bốc đề ngẫu nhiên)**:
   - `student1` bấm "Xem đề" -> chuyển tới trang `SlotDetail`, thấy 2 câu hỏi đã được tự động bốc ngẫu nhiên kèm mức Bloom.
   - Admin tại trang Assignments xem danh sách slot: Slot #1 và Slot #2 có các câu hỏi hoàn toàn khác nhau (không trùng thí sinh liền trước).
   - Admin bấm "Bốc lại đề" cho Slot #1 -> Cửa sổ của `student1` ngay lập tức đổi sang 2 câu hỏi mới tinh với hiệu ứng highlight (SignalR `SlotQuestionsUpdated`).
   - Admin mở "Ngân hàng câu hỏi", thêm 1 câu hỏi mới -> Cửa sổ Admin khác hiện ngay (SignalR `QuestionCreated`); thử xóa câu hỏi đã gán thì hệ thống từ chối xóa và yêu cầu khóa để bảo vệ dữ liệu.
6. **Tổng kết**: Chạy `powershell -NoProfile -File tools/check-layers.ps1` kiểm tra đạt chuẩn kiến trúc 3 lớp không chạm DAL.

