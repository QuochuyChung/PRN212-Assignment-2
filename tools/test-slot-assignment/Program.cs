using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Realtime;
using asg2.BLL.Services;
using asg2.DAL.Data;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("============================================================");
Console.WriteLine(" KIỂM THỬ P4: GÁN SINH VIÊN VÀO PHIÊN / TỰ CHIA KHUNG GIỜ");
Console.WriteLine("============================================================");

var passed = 0;
var total = 0;

void Assert(bool condition, string name)
{
    total++;
    if (!condition)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {name}");
        Console.ResetColor();
        Environment.ExitCode = 1;
        throw new InvalidOperationException(name);
    }

    passed++;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[PASS] {name}");
    Console.ResetColor();
}

async Task<TestContext> SetupAsync(int capacity = 4)
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase($"SlotAssignment_{Guid.NewGuid():N}")
        .Options;
    var db = new AppDbContext(options);

    var subject = new Subject { Code = "PRN222", Name = "Lập trình Web" };
    var room = new Room { Name = "A101", Building = "A", IsActive = true };
    var start = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);
    var session = new ExamSession
    {
        Title = "Vấn đáp PRN222",
        Subject = subject,
        Room = room,
        StartTime = start,
        EndTime = start.AddMinutes(capacity * 15),
        SlotMinutes = 15,
        Status = ExamSessionStatus.Scheduled
    };
    db.AddRange(subject, room, session);
    await db.SaveChangesAsync();

    var notifier = new RecordingNotifier();
    var picker = new RecordingQuestionPicker();
    var service = new SlotService(
        new ExamSlotRepository(db),
        new ExamSessionRepository(db),
        new AccountRepository(db),
        notifier,
        picker);
    return new TestContext(db, session, service, notifier, picker);
}

async Task<Account[]> AddStudentsAsync(TestContext context, params (string Username, bool Active, AccountRole Role)[] specs)
{
    var students = specs.Select(spec => new Account
    {
        Username = spec.Username,
        FullName = $"Sinh viên {spec.Username}",
        PasswordHash = "test-hash",
        IsActive = spec.Active,
        Role = spec.Role
    }).ToArray();
    context.Db.AddRange(students);
    await context.Db.SaveChangesAsync();
    return students;
}

Console.WriteLine("\n--- TỰ CHIA GIỜ VÀ REALTIME ---");
{
    var context = await SetupAsync();
    var students = await AddStudentsAsync(context,
        ("an", true, AccountRole.Student),
        ("binh", true, AccountRole.Student));

    var result = await context.Service.AssignStudentsAsync(context.Session.Id, students.Select(s => s.Id));
    var slots = await context.Db.Set<ExamSlot>().OrderBy(s => s.OrderNo).ToListAsync();

    Assert(result.IsSuccess && slots.Select(s => s.OrderNo).SequenceEqual(new[] { 1, 2 }),
        "Gán nhiều sinh viên và cấp số thứ tự liên tiếp");
    Assert(slots[0].StartTime == context.Session.StartTime
           && slots[1].StartTime == context.Session.StartTime.AddMinutes(15),
        "Tự tính đúng khung giờ theo SlotMinutes");
    Assert(context.Picker.PickedSlotIds.Order().SequenceEqual(slots.Select(s => s.Id).Order()),
        "Gọi bốc câu hỏi cho từng slot sau khi lưu");
    Assert(context.Notifier.UserEvents.Count == 2 && context.Notifier.AdminEvents.Count == 2,
        "Phát realtime cho sinh viên và quản trị viên");
}

Console.WriteLine("\n--- LẤP SLOT TRỐNG VÀ GỠ SINH VIÊN ---");
{
    var context = await SetupAsync();
    var students = await AddStudentsAsync(context,
        ("s1", true, AccountRole.Student),
        ("s2", true, AccountRole.Student),
        ("s3", true, AccountRole.Student));
    var first = await context.Service.AssignStudentsAsync(context.Session.Id, students.Take(2).Select(s => s.Id));
    var removed = first.Value!.Single(slot => slot.OrderNo == 1);

    await context.Service.RemoveStudentAsync(removed.Id);
    var replacement = await context.Service.AssignStudentAsync(context.Session.Id, students[2].Id);

    Assert(replacement.IsSuccess && replacement.Value!.OrderNo == 1,
        "Sau khi gỡ, ưu tiên lấp slot trống có thứ tự nhỏ nhất");
    Assert(context.Notifier.AdminEvents.Contains(HubEvents.SlotRemoved),
        "Gỡ sinh viên phát sự kiện realtime");
}

Console.WriteLine("\n--- VALIDATION ATOMIC ---");
{
    var context = await SetupAsync(capacity: 1);
    var students = await AddStudentsAsync(context,
        ("capacity-a", true, AccountRole.Student),
        ("capacity-b", true, AccountRole.Student));
    var result = await context.Service.AssignStudentsAsync(context.Session.Id, students.Select(s => s.Id));

    Assert(!result.IsSuccess && result.Errors.ContainsKey("Capacity"),
        "Chặn yêu cầu vượt sức chứa");
    Assert(!await context.Db.Set<ExamSlot>().AnyAsync(),
        "Không lưu một phần dữ liệu khi cả nhóm không thể được gán");
}
{
    var context = await SetupAsync();
    var students = await AddStudentsAsync(context,
        ("valid", true, AccountRole.Student),
        ("locked", false, AccountRole.Student),
        ("admin", true, AccountRole.Admin));

    var mixed = await context.Service.AssignStudentsAsync(
        context.Session.Id,
        new[] { students[0].Id, students[1].Id });
    var admin = await context.Service.AssignStudentAsync(context.Session.Id, students[2].Id);

    Assert(!mixed.IsSuccess && !await context.Db.Set<ExamSlot>().AnyAsync(),
        "Tài khoản bị khóa làm toàn bộ yêu cầu nhóm thất bại atomic");
    Assert(!admin.IsSuccess, "Không cho gán tài khoản không có vai trò Student");
}

Console.WriteLine("\n--- GHÉP LỊCH KHÔNG TRÙNG ---");
{
    var context = await SetupAsync(capacity: 2);
    var students = await AddStudentsAsync(context,
        ("flexible", true, AccountRole.Student),
        ("constrained", true, AccountRole.Student));

    var otherSession = new ExamSession
    {
        Title = "Phiên khác",
        SubjectId = context.Session.SubjectId,
        RoomId = context.Session.RoomId,
        StartTime = context.Session.StartTime.AddMinutes(15),
        EndTime = context.Session.StartTime.AddMinutes(30),
        SlotMinutes = 15
    };
    context.Db.Add(otherSession);
    await context.Db.SaveChangesAsync();
    context.Db.Add(new ExamSlot
    {
        SessionId = otherSession.Id,
        StudentId = students[1].Id,
        OrderNo = 1,
        StartTime = otherSession.StartTime,
        EndTime = otherSession.EndTime
    });
    await context.Db.SaveChangesAsync();

    var result = await context.Service.AssignStudentsAsync(
        context.Session.Id,
        new[] { students[0].Id, students[1].Id });
    var assigned = result.Value!.ToDictionary(slot => slot.StudentId);

    Assert(result.IsSuccess && assigned[students[1].Id].OrderNo == 1 && assigned[students[0].Id].OrderNo == 2,
        "Ghép sinh viên bị ràng buộc trước để vẫn xếp được cả nhóm");
}
{
    var context = await SetupAsync(capacity: 2);
    var student = (await AddStudentsAsync(context, ("busy", true, AccountRole.Student))).Single();
    var otherSession = new ExamSession
    {
        Title = "Phiên bao phủ",
        SubjectId = context.Session.SubjectId,
        RoomId = context.Session.RoomId,
        StartTime = context.Session.StartTime,
        EndTime = context.Session.EndTime,
        SlotMinutes = 30
    };
    context.Db.Add(otherSession);
    await context.Db.SaveChangesAsync();
    context.Db.Add(new ExamSlot
    {
        SessionId = otherSession.Id,
        StudentId = student.Id,
        OrderNo = 1,
        StartTime = otherSession.StartTime,
        EndTime = otherSession.EndTime
    });
    await context.Db.SaveChangesAsync();

    var result = await context.Service.AssignStudentAsync(context.Session.Id, student.Id);
    Assert(!result.IsSuccess && result.Errors.ContainsKey("StudentIds"),
        "Chặn sinh viên trùng với tất cả khung giờ còn trống");
}

Console.WriteLine("\n--- TRÙNG TRONG CÙNG PHIÊN ---");
{
    var context = await SetupAsync();
    var student = (await AddStudentsAsync(context, ("duplicate", true, AccountRole.Student))).Single();
    await context.Service.AssignStudentAsync(context.Session.Id, student.Id);
    var duplicate = await context.Service.AssignStudentAsync(context.Session.Id, student.Id);

    Assert(!duplicate.IsSuccess && await context.Db.Set<ExamSlot>().CountAsync() == 1,
        "Chặn gán trùng sinh viên trong cùng phiên");
}

Console.WriteLine($"\nKết quả: {passed}/{total} kiểm thử đạt.");
Environment.ExitCode = passed == total ? 0 : 1;

internal sealed record TestContext(
    AppDbContext Db,
    ExamSession Session,
    ISlotService Service,
    RecordingNotifier Notifier,
    RecordingQuestionPicker Picker);

internal sealed class RecordingNotifier : IRealtimeNotifier
{
    public List<(int AccountId, string EventName)> UserEvents { get; } = new();
    public List<string> AdminEvents { get; } = new();

    public Task BroadcastAsync(string eventName, object? payload) => Task.CompletedTask;

    public Task SendToUserAsync(int accountId, string eventName, object? payload)
    {
        UserEvents.Add((accountId, eventName));
        return Task.CompletedTask;
    }

    public Task SendToAdminsAsync(string eventName, object? payload)
    {
        AdminEvents.Add(eventName);
        return Task.CompletedTask;
    }
}

internal sealed class RecordingQuestionPicker : IQuestionPickerService
{
    public List<int> PickedSlotIds { get; } = new();

    public Task<IReadOnlyList<SlotQuestionDto>> GetQuestionsOfSlotAsync(int slotId) =>
        Task.FromResult<IReadOnlyList<SlotQuestionDto>>(Array.Empty<SlotQuestionDto>());

    public Task<ServiceResult<IReadOnlyList<SlotQuestionDto>>> PickForSlotAsync(int slotId)
    {
        PickedSlotIds.Add(slotId);
        return Task.FromResult(ServiceResult<IReadOnlyList<SlotQuestionDto>>.Success(Array.Empty<SlotQuestionDto>()));
    }

    public Task<ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>> PickForAllSlotsInSessionAsync(int sessionId) =>
        Task.FromResult(ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>.Success(
            new Dictionary<int, IReadOnlyList<SlotQuestionDto>>()));
}
