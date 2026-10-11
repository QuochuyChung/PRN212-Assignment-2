# Asg2 — Kế hoạch chia task (Razor Pages + 3 layer + SignalR), mỗi người một "lát dọc" đầy đủ

> Bản nháp để cả nhóm chốt ở buổi họp đầu. Chỗ đánh dấu **[GIẢ ĐỊNH]** cần xác nhận trước khi chia việc thật.

## 0. Giả định cần chốt

| # | Giả định | Nếu sai thì |
|---|---|---|
| 1 | Luồng chức năng asg2 = **Nhóm 2 – Quản lý kỳ thi và lịch thi** (asg1 làm Nhóm 1) | Đổi domain, giữ nguyên cách chia (mục 11) |
| 2 | Nhóm **5 người** (P1–P5) | Xem mục 11 |
| 3 | Asg2 là **solution mới**, DB riêng, không tham chiếu code asg1 (đề: "kh liên quan asg1") | Chỉ tái dùng pattern, không tham chiếu project |
| 4 | DB **SQL Server dùng chung tại `192.168.58.24`** (user/mật khẩu do leader cấp, ghi trong `appsettings.json` cục bộ, không commit). **Chỉ leader** áp migration + seed lên server chung; mặc định app không tự migrate (cờ `Database:InitializeOnStartup = false`). Ai muốn thử riêng thì dùng LocalDB qua biến môi trường | Xem README mục 3 |

## 1. Đề bài tóm tắt (đối chiếu khi nghiệm thu)

- [ ] **Razor Pages**, code-behind kế thừa `PageModel`, viết gọn như controller (chỉ xử lý UI, gọi service).
- [ ] **3 layer** rõ ràng (không 3 layer = 0 điểm).
- [ ] **SignalR realtime cho CRUD**: admin thêm/sửa/xóa thì client thấy ngay, không reload.
- [ ] Chức năng **khác asg1**, chạy end-to-end để demo.
- [ ] Connection string nằm trong `appsettings.json`.

## 2. Domain: kỳ thi và lịch thi (Nhóm 2)

Admin quản lý **tài khoản sinh viên**, **môn học**, **phòng thi**, **ngân hàng câu hỏi**, tạo **phiên thi** (môn, phòng, khung giờ, thời lượng mỗi thí sinh, số câu hỏi), **gán sinh viên** vào phiên (hệ thống chia khung giờ từng người) và **bốc bộ câu hỏi** riêng cho từng slot, tránh trùng thí sinh liền trước. Sinh viên xem **lịch thi chung**, **lịch thi của mình** và câu hỏi của slot mình; mọi thay đổi của admin **hiện ra ngay**.

Vai trò: `Admin` (CRUD), `Student` (xem). Đăng nhập bằng username + mật khẩu, tài khoản mẫu do seeder tạo.

## 3. Nguyên tắc chia: mỗi người sở hữu một lát dọc, không chia theo tầng

Chia theo tầng (một người DAL, một người UI, một người test...) khiến có người chỉ viết giao diện hoặc chỉ ngồi test, nhìn vào git là thấy lệch. Ở đây **mỗi người sở hữu một tính năng từ DB đến giao diện và SignalR**, nên ai cũng có commit ở cả 3 layer và có phần realtime của riêng mình:

| | DAL | BLL | Web | SignalR server (bắn event) | SignalR client (JS) | Test và tài liệu |
|---|---|---|---|---|---|---|
| **P1** Tài khoản + đăng nhập | `Account` + repo | `AuthService`, `AccountService` | Login/Logout, `Admin/Accounts`, `_Layout` | `Account*`, `ForceLogout` | Danh sách tài khoản realtime, tự đăng xuất | Test + README phần setup/auth |
| **P2** Môn, phòng, SignalR core | `Subject`, `Room` + repo | `SubjectService`, `RoomService`, `RealtimeNotifier` | `Admin/Subjects`, `Admin/Rooms`, `Admin/Dashboard`, `ExamHub` | `Subject*`, `Room*` | `exam-hub.js` dùng chung, dashboard cập nhật live | Test + README phần realtime |
| **P3** Phiên thi | `ExamSession` + repo | `ExamSessionService` | `Admin/Sessions`, `Student/Schedule` | `Session*` | Lịch thi của sinh viên cập nhật live | Test + README phần phiên thi |
| **P4** Gán sinh viên và slot | `ExamSlot` + repo | `SlotService` | `Admin/Assignments`, `Student/MySlots` | `SlotAssigned/Removed` | "Lịch của tôi" cập nhật live | Test + README phần slot |
| **P5** Ngân hàng câu hỏi và bốc câu | `Question`, `SlotQuestion` + repo | `QuestionService`, `QuestionPickerService` | `Admin/Questions`, `Student/SlotDetail` | `Question*`, `SlotQuestionsUpdated` | Câu hỏi của slot cập nhật live | Test + README phần câu hỏi |

Độ nặng cân bằng nhau: P1 và P2 gánh thêm hạ tầng (khung dự án, đăng nhập, lõi SignalR), P3–P5 gánh nghiệp vụ nặng hơn (validate, chia slot, bốc câu).

## 4. Kiến trúc và quy ước chung

```
asg2.sln
├── asg2.Web   Razor Pages + ExamHub + SignalRNotifier          (Presentation)
├── asg2.BLL   Services, DTO, ServiceResult, IRealtimeNotifier  (Business)
└── asg2.DAL   Entities, AppDbContext, Repositories, Migrations (Data)
```

1. Phụ thuộc một chiều **Web → BLL → DAL**; entity không rời DAL, BLL trả DTO. **Tầng Web tuyệt đối không được chạm DAL** — xem ràng buộc 4.1.
2. **PageModel chỉ gọi Service (BLL)**: bind input, kiểm `ModelState`, gọi service, xử lý `ServiceResult`. Không `DbContext`, không nghiệp vụ trong PageModel.
3. **Realtime qua `IRealtimeNotifier`** (interface ở BLL, cài đặt ở Web bằng `IHubContext<ExamHub>`). **Service gọi notifier sau khi lưu DB thành công.** BLL không phụ thuộc SignalR; ai làm CRUD cũng chỉ gọi một dòng, không đụng Hub.
4. `ExamHub` **chỉ đẩy xuống client**, không có method client gọi lên.
5. **Đọc chéo bảng thì được, ghi chéo bảng phải đi qua service của chủ bảng.** Ví dụ P2 kiểm "phòng đang được dùng" bằng cách đọc bảng `ExamSessions`; nhưng không tự ý sửa dữ liệu của P3.
6. Connection string chỉ ở `appsettings.json`. File thật bị `.gitignore`; repo chỉ có `appsettings.example.json`.
7. Mỗi entity có `IEntityTypeConfiguration<T>` riêng trong file riêng (`ApplyConfigurationsFromAssembly`), nên `AppDbContext` gần như không bị sửa.

### 4.1 Ràng buộc tuyệt đối: tầng Web không được gọi xuống DAL, bằng bất kỳ cách nào

**Không có ngoại lệ**, kể cả `Program.cs`, `ExamHub`, `SignalRNotifier`, PageModel, view, JS handler, seeder khởi động. Vi phạm = PR bị từ chối (cũng là điều kiện "không 3 layer = 0 điểm").

Trong code của `asg2.Web` **cấm**:
- `using asg2.DAL...`, hoặc dùng entity, `AppDbContext`, Repository, Migration, seeder của DAL.
- Dùng EF Core (`Microsoft.EntityFrameworkCore...`), kể cả `ToListAsync`, `Include`, `FromSqlRaw`.
- Tự đọc connection string để mở DB (SqlConnection, ADO.NET, Dapper...).
- Dùng reflection / `GetService` để lấy Repository hoặc DbContext, hay `typeof` / `Assembly.Load` trỏ vào DAL.
- Lấy dữ liệu "cho nhanh" bằng cách đọc thẳng DB ở handler của dashboard, hub, hay trang nào đó. Cần dữ liệu thì thêm method vào Service ở BLL.

**Khóa bằng kỹ thuật (đã thử trên máy: Web không còn thấy kiểu của DAL và EF Core, app vẫn chạy, `dotnet ef` vẫn chạy):**

1. `asg2.Web.csproj` **chỉ** có `ProjectReference` tới BLL, **không bao giờ** tới DAL.
2. `asg2.BLL.csproj` tham chiếu DAL với `PrivateAssets="compile"`, đúng chính tả và đúng thứ tự thuộc tính như dưới đây (script kiểm tra dựa vào dạng này):
   ```xml
   <ProjectReference Include="..\asg2.DAL\asg2.DAL.csproj" PrivateAssets="compile" />
   ```
   Nhờ vậy kiểu của DAL (và EF Core) **không "chảy" sang Web** lúc biên dịch: Web gõ `using asg2.DAL` hay `DbContext` là **lỗi biên dịch**. Lúc chạy DLL vẫn được copy bình thường.
3. Web không cài package EF Core nào, **trừ** `Microsoft.EntityFrameworkCore.Design` (chỉ để chạy `dotnet ef` với Web là startup project), khai báo `PrivateAssets="all"`.
4. **Đăng ký DI:** BLL cung cấp đúng **một** extension `AddBusinessLayer(this IServiceCollection, IConfiguration)`; bên trong nó mới gọi DAL (`AddDbContext`, repository, seeder). `Program.cs` của Web chỉ gọi `AddBusinessLayer`.
5. **Migrate và seed lúc khởi động:** BLL cung cấp `InitializeDatabaseAsync(this IServiceProvider)` (bên trong gọi Migrate + seeder của DAL). Web chỉ gọi hàm này.
6. **API công khai của BLL không được lộ kiểu DAL:** không nhận/trả entity, `DbContext`, Repository trong chữ ký `public`; chỉ DTO, enum của BLL và kiểu nguyên thủy. Trang nào cần dữ liệu thì xin qua Service (ví dụ thống kê dashboard là `IDashboardService`, không phải đếm trực tiếp trong PageModel).
7. Lệnh migration: `dotnet ef migrations add <Tên> --project asg2.DAL --startup-project asg2.Web`.

**Kiểm tra tự động:** P1 đặt script bên dưới ở `tools/check-layers.ps1`. **Mọi người chạy nó trước khi mở PR; reviewer chạy lại khi duyệt.** Script thoát mã 1 nếu Web tham chiếu DAL, BLL thiếu `PrivateAssets="compile"`, Web cài package EF Core ngoài Design, hoặc file `.cs`/`.cshtml` trong Web chứa `asg2.DAL`, `DbContext`, `Microsoft.EntityFrameworkCore`, `Repository`, `Migration` (đã chạy thử các ca sạch và các ca vi phạm).

```powershell
# tools/check-layers.ps1 - chay o thu muc goc repo truoc khi mo PR. Thoat ma 1 neu vi pham.
$ErrorActionPreference = 'Stop'
$bad = @()

$web = Get-Content asg2.Web/asg2.Web.csproj -Raw
$bll = Get-Content asg2.BLL/asg2.BLL.csproj -Raw

if ($web -match 'asg2\.DAL') { $bad += 'asg2.Web.csproj tham chieu asg2.DAL' }
if ($bll -notmatch 'ProjectReference Include="[^"]*asg2\.DAL[^"]*"\s+PrivateAssets="compile"') {
    $bad += 'asg2.BLL.csproj thieu PrivateAssets="compile" khi tham chieu asg2.DAL'
}
if ($web -match 'Microsoft\.EntityFrameworkCore(?!\.Design)') { $bad += 'asg2.Web.csproj cai package EF Core ngoai Design' }

Get-ChildItem asg2.Web -Recurse -Include *.cs,*.cshtml |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    Select-String -Pattern 'asg2\.DAL|DbContext|Microsoft\.EntityFrameworkCore|Repository|\bMigrations?\b' |
    ForEach-Object { $bad += "$($_.Path):$($_.LineNumber): $($_.Line.Trim())" }

if ($bad.Count -gt 0) {
    $bad | ForEach-Object { Write-Host "VI PHAM: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'OK: tang Web khong cham DAL' -ForegroundColor Green
```

## 5. Hợp đồng chung — chốt ở buổi họp đầu, sau đó không tự ý đổi

### 5.1 Entity (mỗi người tự viết entity của mình)
| Entity (chủ) | Cột chính |
|---|---|
| `Account` (P1) | `Id` int, `Username` (unique), `PasswordHash`, `FullName`, `Role` (`Admin`/`Student`), `IsActive` |
| `Subject` (P2) | `Id`, `Code` (unique), `Name` |
| `Room` (P2) | `Id`, `Name` (unique), `Building`, `IsActive` |
| `Question` (P5) | `Id`, `SubjectId`, `Content`, `BloomLevel` (1–4), `IsActive` |
| `ExamSession` (P3) | `Id`, `Title`, `SubjectId`, `RoomId`, `StartTime`, `EndTime`, `SlotMinutes`, `MainQuestionCount`, `MaxFollowUpCount`, `Status` (`Scheduled`/`Cancelled`/`Completed`), `CreatedAt` |
| `ExamSlot` (P4) | `Id`, `SessionId`, `StudentId` (FK `Account`), `OrderNo`, `StartTime`, `EndTime` |
| `SlotQuestion` (P5) | (`SlotId`, `QuestionId`) PK ghép, `OrderNo` |

Quy tắc dữ liệu đã chốt:
- Sức chứa phiên = `floor((EndTime − StartTime) / SlotMinutes)`. Slot thứ n bắt đầu lúc `StartTime + (n−1) × SlotMinutes`; slot trống được người mới lấp lại, nhỏ nhất trước.
- Hai phiên **cùng phòng không được chồng khung giờ**. Một sinh viên **không được có hai slot chồng giờ** (kể cả khác phiên).
- Xóa môn/phòng/tài khoản/câu hỏi **đang được dùng** thì bị chặn kèm thông báo "đang được dùng bởi N ..." (câu hỏi dùng rồi thì khóa `IsActive = false`, không xóa).
- Thời gian lưu cùng một kiểu (UTC) cho cả nhóm.

### 5.2 Service interface (mỗi người viết interface và DTO của mình **trước**, merge sớm, rồi mới cài đặt)
| Chủ | Interface | Method dùng chéo |
|---|---|---|
| P1 | `IAuthService`, `IAccountService` | `GetStudentsAsync()` → cho P4 chọn sinh viên |
| P2 | `ISubjectService`, `IRoomService`, `IDashboardService`, `IRealtimeNotifier` | `GetAllAsync()` → cho P3/P5 dropdown; `GetStatsAsync()` → số liệu dashboard (đọc các bảng ở DAL qua repo, Web không tự đếm) |
| P3 | `IExamSessionService` | `GetUpcomingAsync()`, `GetByIdAsync(id)` |
| P4 | `ISlotService` | `GetSlotsOfSessionAsync`, `GetMySlotsAsync(studentId)`, `AssignStudentsAsync`, `RemoveStudentAsync`, `RecalculateSlotsAsync(sessionId)`, `RemoveAllOfSessionAsync(sessionId)` |
| P5 | `IQuestionService`, `IQuestionPickerService` | `PickForSlotAsync(slotId)`, `GetQuestionsOfSlotAsync(slotId)` |

Mọi service trả `ServiceResult` / `ServiceResult<T>` (lỗi theo key, giống pattern asg1).

Các điểm gọi chéo đã chốt: P3 gọi `RecalculateSlotsAsync` khi đổi giờ phiên và `RemoveAllOfSessionAsync` khi xóa phiên · P4 gọi `PickForSlotAsync` sau khi gán sinh viên.

### 5.3 SignalR — hub `/hubs/exam` (`[Authorize]`), notifier tổng quát
`IRealtimeNotifier` chỉ có 3 hàm, nên không ai phải sửa interface của người khác:
```
BroadcastAsync(string eventName, object payload)            // mọi client đang đăng nhập
SendToUserAsync(int accountId, string eventName, object payload)
SendToAdminsAsync(string eventName, object payload)         // group "admins"
```
Tên event là hằng số trong `HubEvents` (P2 tạo sẵn đủ danh sách dưới đây ở Sprint 1):

| Event | Payload | Gửi tới | Chủ |
|---|---|---|---|
| `AccountCreated/Updated/Deleted` | `AccountDto` / `id` | admins | P1 |
| `ForceLogout` | — | `SendToUserAsync` (tài khoản bị khóa/xóa) | P1 |
| `SubjectCreated/Updated/Deleted`, `RoomCreated/Updated/Deleted` | DTO / `id` | admins | P2 |
| `SessionCreated/Updated/Deleted` | `SessionDto` / `id` | mọi client | P3 |
| `SlotAssigned` / `SlotRemoved` | `SlotDto` / `slotId, sessionId` | sinh viên đó + admins | P4 |
| `QuestionCreated/Updated/Deleted` | `QuestionDto` / `id` | admins | P5 |
| `SlotQuestionsUpdated` | `slotId, QuestionDto[]` | sinh viên đó + admins | P5 |

- Claim `NameIdentifier` = `Account.Id` (để gửi theo user), claim `Role` = `Admin`/`Student`; `OnConnectedAsync` cho Admin vào group `admins`.
- JS dùng chung `wwwroot/js/exam-hub.js` (P2): tạo **một** connection và cho đăng ký `ExamEvents.on("SessionCreated", handler)`. Trang nào cũng chỉ đăng ký handler, không tự tạo connection mới.

### 5.4 Thư mục Pages — mỗi người một thư mục riêng
```
Pages/Account/            Login, Logout                              P1
Pages/Admin/Accounts/     CRUD tài khoản sinh viên                   P1
Pages/Shared/             _Layout, partial dùng chung (toast)        P1
Pages/Admin/Subjects/     CRUD môn học                               P2
Pages/Admin/Rooms/        CRUD phòng                                 P2
Pages/Admin/Dashboard     thống kê live                              P2
Pages/Admin/Sessions/     CRUD phiên thi                             P3
Pages/Student/Schedule    lịch thi chung                             P3
Pages/Admin/Assignments/  gán sinh viên, xem slot                    P4
Pages/Student/MySlots     lịch của tôi                               P4
Pages/Admin/Questions/    CRUD ngân hàng câu hỏi                     P5
Pages/Student/SlotDetail  câu hỏi của slot                           P5
```

## 6. Phân công chi tiết

Mỗi người làm theo cùng một thứ tự **A → F** cho lát của mình: A. Entity + config + interface + DTO (merge sớm) · B. Repository · C. Service + validate · D. Pages (PageModel gọn) · E. Bắn event + JS nhận event · F. Test + README.

### P1 — Tài khoản, đăng nhập, khung dự án
- [x] **Khung dự án (làm trước tiên, vài giờ):** solution 3 project, reference đúng chiều, `.gitignore`, `appsettings.example.json`, `ServiceResult`, README cách chạy. Đẩy lên `develop` sớm nhất để cả nhóm clone. ✅ (solution `.sln` thay vì `.slnx` để SDK 8 cũng mở được)
- [x] **Khóa ràng buộc 4.1 ngay từ khung:** `asg2.Web.csproj` không tham chiếu DAL; `asg2.BLL.csproj` có `PrivateAssets="compile"`; Web chỉ cài `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets="all"`); `AddBusinessLayer(IConfiguration)` và `InitializeDatabaseAsync(IServiceProvider)` ở BLL, `Program.cs` chỉ gọi hai hàm đó; thêm `tools/check-layers.ps1`. Chạy script ngay trên khung rồi mới đẩy lên `develop`. ✅ (đã chạy `check-layers.ps1`; thử gõ `using asg2.DAL` trong Web ra lỗi biên dịch)
- [x] A–B: `Account` + config + `IAccountRepository`; mật khẩu băm bằng `PasswordHasher`. ✅
- [x] C: `AuthService.LoginAsync` (username/mật khẩu, tài khoản `IsActive`); `AccountService` CRUD sinh viên (username unique, định dạng, không xóa tài khoản đang có slot, khóa thay vì xóa). ✅
- [x] D: Login/Logout, cookie auth + role (`Admin` cho `Pages/Admin`, `Student` cho `Pages/Student`), `Admin/Accounts` (Index, Create, Edit, Delete/khóa), `_Layout` menu theo role. ✅
- [x] E: bắn `Account*` khi CRUD; khi khóa/xóa tài khoản đang online → `ForceLogout`; JS: bảng tài khoản tự cập nhật, nhận `ForceLogout` thì về trang Login. ✅ phía server và JS xong, **chờ P2** thay stub `exam-hub.js` bằng kết nối SignalR thật thì event mới tới trình duyệt (khóa/xóa tài khoản vẫn có hiệu lực ở request kế tiếp)
- [x] F: seeder tài khoản mẫu (1 admin, ~10 sinh viên); test các case đăng nhập/phân quyền; README phần setup + tài khoản mẫu. ✅ (đã chạy 20 ca HTTP end-to-end + kiểm JS; script test chưa đưa vào repo); mật khẩu mẫu ở README
**Xong khi:** `dotnet run` ra trang Login, đăng nhập đúng role, sinh viên vào `Pages/Admin` bị chặn, admin khóa một sinh viên đang online thì sinh viên đó bị đăng xuất ngay.

### P2 — Môn học, phòng thi, lõi SignalR, dashboard
- [ ] **Lõi SignalR (làm sớm, Sprint 1):** `ExamHub`, `IRealtimeNotifier` + `SignalRNotifier`, `HubEvents` (đủ danh sách mục 5.3), `exam-hub.js`, toast dùng chung (phối hợp P1 ở `_Layout`).
- [ ] A–B: `Subject`, `Room` + config + repo.
- [ ] C: `SubjectService`, `RoomService` (mã môn/tên phòng unique, chặn xóa khi đang dùng, `GetAllAsync()` cho dropdown); `DashboardService.GetStatsAsync()` trả `DashboardStatsDto` (số phiên, sinh viên, phòng, câu hỏi, đếm qua repo; nếu cần repo của chủ khác thì chỉ **đọc**).
- [ ] D: `Admin/Subjects`, `Admin/Rooms` (Index, Create, Edit, Delete); `Admin/Dashboard` (PageModel chỉ gọi `IDashboardService`, có handler `OnGetStats` trả JSON từ DTO).
- [ ] E: bắn `Subject*`/`Room*`; dashboard nghe mọi event admin và gọi lại `?handler=Stats` để đổi số **live**.
- [ ] F: seeder môn và phòng mẫu; test; README phần realtime (sơ đồ luồng PageModel → Service → Notifier → Hub → JS).
**Xong khi:** mở Dashboard ở một cửa sổ, cửa sổ khác thêm/xóa môn hoặc phòng thì danh sách và số liệu đổi ngay; event bắn đúng payload mục 5.3.

### P3 — Phiên thi + lịch thi chung của sinh viên
- [ ] A–B: `ExamSession` + config + repo (lọc theo môn, trạng thái, tìm theo tên).
- [ ] C: `ExamSessionService` — validate: tiêu đề bắt buộc, `EndTime > StartTime`, chia được ≥ 1 slot, **không chồng giờ cùng phòng**, môn/phòng tồn tại và `IsActive`, số câu hỏi ≥ 1. Đổi giờ → gọi `RecalculateSlotsAsync` (P4); xóa phiên → gọi `RemoveAllOfSessionAsync` (P4). Trong lúc P4 chưa xong dùng stub.
- [ ] D: `Admin/Sessions` (Index có lọc, Create/Edit với dropdown môn + phòng lấy từ P2, Details, Delete có xác nhận); `Student/Schedule` (bảng phiên sắp tới, lọc theo môn).
- [ ] E: bắn `Session*`; `schedule.js` thêm/cập nhật/xóa dòng realtime kèm highlight và toast "Phiên X đã bị hủy"; trạng thái kết nối + tải lại dữ liệu sau khi reconnect.
- [ ] F: test các case chồng giờ; README phần phiên thi.
- Trong lúc chờ P1/P2: làm UI bằng dữ liệu giả cứng đúng cấu trúc `SessionDto`, rồi nối thật.
**Xong khi:** admin thêm/sửa/xóa phiên → cửa sổ sinh viên thấy tức thì; mọi validate chặn đúng và báo lỗi rõ.

### P4 — Gán sinh viên và chia khung giờ
- [ ] A–B: `ExamSlot` + config + repo.
- [ ] C: `SlotService`: `AssignStudentsAsync` (nhiều sinh viên, cấp slot theo quy tắc 5.1, chặn vượt sức chứa / trùng giờ / trùng gán), `RemoveStudentAsync`, `RecalculateSlotsAsync`, `RemoveAllOfSessionAsync`, `GetMySlotsAsync`. Sau khi gán xong gọi `PickForSlotAsync` (P5).
- [ ] D: `Admin/Assignments` (chọn phiên, tick nhiều sinh viên lấy từ P1, bảng slot theo thứ tự, nút gỡ); `Student/MySlots`.
- [ ] E: bắn `SlotAssigned`/`SlotRemoved` đúng người nhận; `my-slots.js` thêm/bỏ dòng realtime; khi đổi giờ phiên thì giờ slot của sinh viên đổi ngay.
- [ ] F: test các case trùng giờ/vượt sức chứa; README phần slot.
**Xong khi:** gán sinh viên → "Lịch của tôi" của họ hiện slot ngay; gỡ thì biến mất ngay; đổi giờ phiên thì giờ slot đổi theo.

### P5 — Ngân hàng câu hỏi và bốc câu hỏi
- [ ] A–B: `Question`, `SlotQuestion` + config + repo.
- [ ] C: `QuestionService` (CRUD, nội dung không rỗng, chặn trùng nội dung trong cùng môn, khóa thay vì xóa nếu đã dùng); `QuestionPickerService`: bốc **ngẫu nhiên** `MainQuestionCount` câu cho mỗi slot, **tránh trùng** slot liền trước khi còn đủ câu (không đủ thì trùng tối thiểu và ghi chú), lưu `SlotQuestion`; `GetQuestionsOfSlotAsync`.
- [ ] D: `Admin/Questions` (Index lọc theo môn/mức Bloom, Create, Edit, khóa); nút "Bốc lại câu hỏi" ở trang Assignments (phối hợp P4); `Student/SlotDetail` (câu hỏi của slot mình).
- [ ] E: bắn `Question*`, `SlotQuestionsUpdated`; JS: trang `SlotDetail` tự cập nhật khi admin bốc lại.
- [ ] F: seeder ~10 câu/môn (đủ để bốc không hết câu); test thuật toán không trùng; README phần câu hỏi.
**Xong khi:** mỗi slot có đúng `MainQuestionCount` câu, hai slot liền nhau không trùng câu (khi đủ ngân hàng), bốc lại thì sinh viên thấy ngay.

## 7. Việc chung chia đều (để ai cũng có phần)

| Việc | Ai làm |
|---|---|
| Review chéo PR (xoay vòng) | P1 duyệt P2 · P2 duyệt P3 · P3 duyệt P4 · P4 duyệt P5 · P5 duyệt P1 |
| Seeder | Mỗi người seeder cho entity của mình; `DbSeeder` chỉ gọi lần lượt |
| README | Mỗi người viết mục của mình (mục F ở trên); P1 ghép lại |
| Test toàn luồng khi tích hợp | Mỗi người chạy luồng của mình, đổi chéo người chạy luồng demo |
| Trình bày demo | Mỗi người **tự demo phần của mình** (mục 9) |
| Báo cáo / slide (nếu thầy yêu cầu) | Mỗi người 1–2 slide về lát của mình |

## 8. Thứ tự và phụ thuộc

```
Sprint 0  Họp chốt hợp đồng (mục 5), chia repo/branch                         ← cả nhóm
Sprint 1  P1: khung dự án (đẩy lên đầu tiên)
          Cả nhóm: PR "A" của mình (entity + interface + DTO), merge trong ngày
          P1: tạo migration InitialCreate khi mọi entity đã merge
          P2: lõi SignalR + HubEvents
Sprint 2  5 lát chạy song song (B → F). Phụ thuộc: P3 cần Subject/Room (P2), P4 cần
          Session (P3) + sinh viên (P1), P5 cần Slot (P4). Chưa có thì dùng seeder
          hoặc stub; khi bạn kia merge thì nối thật.
Sprint 3  Tích hợp, test toàn luồng, sửa bug, tập demo 3 lần
```

- **Interface-first**: PR đầu tiên của mỗi người chỉ có entity, config, interface, DTO (chưa cài đặt) → ai cũng compile được dựa trên hợp đồng.
- **Migration (DB chung ở `192.168.58.24`)**: ai đổi schema thì tạo migration trong nhánh của mình (`migrations add` chỉ sinh file, không chạm DB), **pull `develop` mới nhất, xóa migration của mình rồi `migrations add` lại** trước khi mở PR để không đụng `ModelSnapshot`. **Chỉ P1 (leader) chạy `database update` / bật seed lên server chung**, sau khi PR đã merge. Chi tiết ở README mục 3.

## 9. Kịch bản demo (≈7 phút, mỗi người trình bày phần của mình)

1. **P1**: đăng nhập admin (cửa sổ A) và `student1` (cửa sổ B, ẩn danh); chỉ ra sinh viên không vào được trang admin.
2. **P2**: mở Dashboard ở một tab admin thứ hai (C); A thêm một phòng mới trên trang Rooms → số liệu và danh sách ở C đổi ngay. Giải thích luồng PageModel → Service → Notifier → Hub.
3. **P3**: A **tạo** phiên thi → B ở trang "Lịch thi" thấy dòng mới hiện ngay; A **sửa** giờ → B đổi ngay; A **xóa** → B thấy dòng biến mất + toast.
4. **P4**: A **gán `student1`** vào phiên → B mở "Lịch của tôi" thấy slot ngay; gỡ thì mất ngay.
5. **P5**: trang slot của `student1` hiện bộ câu hỏi; A bấm "Bốc lại" → B thấy câu hỏi đổi; chỉ ra hai slot liền nhau không trùng câu.
6. **P1** chốt: mở code cho thầy xem 3 layer, connection string trong `appsettings.json`, khóa tài khoản đang online → bị đăng xuất ngay.

## 10. Checklist nghiệm thu chung

- [ ] Build toàn solution 0 lỗi, 0 warning; chạy được từ máy sạch theo README.
- [ ] 3 layer đúng chiều; **tầng Web không chạm DAL (mục 4.1)**: `tools/check-layers.ps1` chạy OK; `asg2.Web.csproj` không có `asg2.DAL`; thử gõ `using asg2.DAL` trong Web phải **lỗi biên dịch**; chữ ký `public` của BLL không có kiểu entity/DbContext/Repository.
- [ ] Mỗi CRUD của admin (tài khoản, môn, phòng, phiên, câu hỏi) → client liên quan **thấy ngay** (kiểm bằng 2 cửa sổ).
- [ ] Gán/gỡ sinh viên → "Lịch của tôi" cập nhật ngay; bốc lại câu hỏi → trang slot cập nhật ngay.
- [ ] Chặn đúng: chồng giờ cùng phòng, vượt sức chứa, sinh viên trùng giờ, xóa dữ liệu đang được dùng; thông báo lỗi rõ.
- [ ] Sinh viên không vào được trang Admin; admin khóa tài khoản đang online thì tài khoản đó bị đăng xuất.
- [ ] Mất kết nối rồi nối lại: dữ liệu trang tải lại đúng.
- [ ] Nội dung có `<script>` hiển thị như chữ, không chạy.
- [ ] Connection string chỉ ở `appsettings.json`; `appsettings.json` thật không bị commit.
- [ ] Mỗi thành viên có commit ở **cả 3 layer** và ít nhất một event SignalR của riêng mình (xem bảng đóng góp mục 12).

## 11. Nếu số người khác 5

| Số người | Cách gộp |
|---|---|
| 4 | P2 giữ lõi SignalR + môn + phòng; **gộp P5 vào P4** (câu hỏi và bốc câu đi liền với slot) |
| 3 | P1 = tài khoản + môn + phòng + lõi SignalR · P2 = phiên thi + slot · P3 = câu hỏi + bốc câu + dashboard |
| 6+ | Tách `Admin/Dashboard` và `Student/*` ra người thứ 6, hoặc tách `Subject` khỏi `Room` |

Nếu **chọn nhóm chức năng khác**, giữ nguyên khung: mỗi người một lát dọc có entity, service, page, event realtime và test riêng; P1 + P2 gánh hạ tầng.

## 12. Git và cách ghi nhận đóng góp

- Một repo chung; `main` + `develop`; mỗi người nhánh `feature/p<N>-<tên>`; xong mở **PR vào `develop`**, người được phân công ở mục 7 duyệt trước khi merge.
- Commit có tiền tố, ví dụ `[P3][BLL] validate trùng phòng`, `[P3][Web] trang Schedule`, `[P3][JS] nhận SessionDeleted` → nhìn history là thấy ai làm tầng nào.
- File dùng chung (`Program.cs`, `AppDbContext`, `_Layout`, `ExamHub`, `exam-hub.js`) chỉ P1/P2 sửa; cần đổi thì mở PR nhỏ và nhắn họ. Mỗi người chỉ sửa thư mục của mình (mục 5.4).
- Không commit `appsettings.json` thật, `bin/`, `obj/`.
- **Mọi PR phải chạy `tools/check-layers.ps1` OK** (dán kết quả vào mô tả PR). Reviewer chạy lại; thấy Web chạm DAL dưới bất kỳ hình thức nào thì từ chối PR, không bàn cãi.
- Muốn đổi schema/DTO/event: báo cả nhóm trước, sửa ở PR riêng.

**Bảng đóng góp** (cả nhóm điền dần, dùng khi thầy hỏi):

| Thành viên | Lát | Entity | Service | Pages | Event SignalR | JS client | Số PR / commit |
|---|---|---|---|---|---|---|---|
| P1 | Tài khoản | | | | | | |
| P2 | Môn, phòng, lõi hub | | | | | | |
| P3 | Phiên thi | | | | | | |
| P4 | Slot | | | | | | |
| P5 | Câu hỏi | | | | | | |
