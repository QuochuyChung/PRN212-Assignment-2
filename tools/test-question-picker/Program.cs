using asg2.BLL.Dtos;
using asg2.BLL.Realtime;
using asg2.BLL.Services;
using asg2.DAL.Data;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("======================================================================");
Console.WriteLine(" BẮT ĐẦU KIỂM THỬ TỰ ĐỘNG: NGÂN HÀNG CÂU HỎI & THUẬT TOÁN BỐC ĐỀ (P5) ");
Console.WriteLine("======================================================================");

var passed = 0;
var total = 0;

void Assert(bool condition, string testName)
{
    total++;
    if (condition)
    {
        passed++;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[PASS] {testName}");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[FAIL] {testName}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}

async Task<(AppDbContext Db, IServiceProvider Provider)> SetupAsync(string dbName)
{
    var services = new ServiceCollection();
    services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase(dbName));

    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddScoped<ISubjectRepository, SubjectRepository>();
    services.AddScoped<IRoomRepository, RoomRepository>();
    services.AddScoped<IExamSessionRepository, ExamSessionRepository>();
    services.AddScoped<IExamSlotRepository, ExamSlotRepository>();
    services.AddScoped<IQuestionRepository, QuestionRepository>();
    services.AddScoped<ISlotQuestionRepository, SlotQuestionRepository>();

    services.AddSingleton<IRealtimeNotifier, NullRealtimeNotifier>();

    services.AddScoped<IQuestionService, QuestionService>();
    services.AddScoped<IQuestionPickerService, QuestionPickerService>();

    var provider = services.BuildServiceProvider();
    var db = provider.GetRequiredService<AppDbContext>();

    return (db, provider);
}

// ---------------------------------------------------------------------------------
// TEST 1: Thêm câu hỏi, validate rỗng, validate mức Bloom, validate trùng nội dung
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 1: QUESTION SERVICE CRUD & VALIDATION ---");
{
    var (db, provider) = await SetupAsync("Db_Test1");
    var qService = provider.GetRequiredService<IQuestionService>();

    var sub = new Subject { Code = "PRN222", Name = "Lập trình .NET" };
    db.Set<Subject>().Add(sub);
    await db.SaveChangesAsync();

    // 1.1 Thêm câu hỏi rỗng -> Thất bại
    var failResult1 = await qService.CreateAsync(new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "   ",
        BloomLevel = 1
    });
    Assert(!failResult1.IsSuccess && failResult1.Errors.ContainsKey("Content"), "Chặn nội dung câu hỏi rỗng");

    // 1.2 Mức Bloom không hợp lệ (< 1 hoặc > 4) -> Thất bại
    var failResult2 = await qService.CreateAsync(new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "Câu hỏi mẫu?",
        BloomLevel = 5
    });
    Assert(!failResult2.IsSuccess && failResult2.Errors.ContainsKey("BloomLevel"), "Chặn mức Bloom ngoài khoảng 1..4");

    // 1.3 Thêm câu hỏi hợp lệ -> Thành công
    var successResult = await qService.CreateAsync(new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "SignalR hoạt động như thế nào?",
        BloomLevel = 2,
        IsActive = true
    });
    Assert(successResult.IsSuccess && successResult.Value!.Id > 0, "Tạo câu hỏi hợp lệ thành công");

    // 1.4 Thêm câu hỏi trùng nội dung trong cùng môn -> Thất bại
    var dupResult = await qService.CreateAsync(new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "SignalR hoạt động như thế nào?",
        BloomLevel = 3
    });
    Assert(!dupResult.IsSuccess && dupResult.Errors.ContainsKey("Content"), "Chặn trùng nội dung câu hỏi trong cùng một môn");
}

// ---------------------------------------------------------------------------------
// TEST 2: Thuật toán bốc đề tránh trùng giữa 2 slot liền kề khi ngân hàng đủ câu
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 2: THUẬT TOÁN TRÁNH TRÙNG 2 SLOT LIỀN KỀ ---");
{
    var (db, provider) = await SetupAsync("Db_Test2");
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "PRN222", Name = "Lập trình .NET" };
    var room = new Room { Name = "A101", Building = "Tòa A", IsActive = true };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);
    await db.SaveChangesAsync();

    // Thêm 10 câu hỏi vào ngân hàng
    for (int i = 1; i <= 10; i++)
    {
        db.Set<Question>().Add(new Question
        {
            SubjectId = sub.Id,
            Content = $"Câu hỏi kiểm tra số {i}?",
            BloomLevel = (i % 4) + 1,
            IsActive = true
        });
    }

    var student1 = new Account { Username = "student1", PasswordHash = "hash", FullName = "Nguyễn Văn A" };
    var student2 = new Account { Username = "student2", PasswordHash = "hash", FullName = "Trần Thị B" };
    db.Set<Account>().AddRange(student1, student2);

    var session = new ExamSession
    {
        Title = "Vấn đáp PRN222",
        SubjectId = sub.Id,
        RoomId = room.Id,
        MainQuestionCount = 2,
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddHours(2)
    };
    db.Set<ExamSession>().Add(session);
    await db.SaveChangesAsync();

    var slot1 = new ExamSlot
    {
        SessionId = session.Id,
        StudentId = student1.Id,
        OrderNo = 1,
        StartTime = session.StartTime,
        EndTime = session.StartTime.AddMinutes(15)
    };
    var slot2 = new ExamSlot
    {
        SessionId = session.Id,
        StudentId = student2.Id,
        OrderNo = 2,
        StartTime = session.StartTime.AddMinutes(15),
        EndTime = session.StartTime.AddMinutes(30)
    };
    db.Set<ExamSlot>().AddRange(slot1, slot2);
    await db.SaveChangesAsync();

    // Chạy kiểm thử 50 lần liên tiếp để đảm bảo tính ngẫu nhiên và tuyệt đối không trùng
    var allZeroOverlap = true;
    for (int run = 0; run < 50; run++)
    {
        var res1 = await picker.PickForSlotAsync(slot1.Id);
        var res2 = await picker.PickForSlotAsync(slot2.Id);

        if (!res1.IsSuccess || !res2.IsSuccess)
        {
            allZeroOverlap = false;
            break;
        }

        var q1Ids = res1.Value!.Select(q => q.QuestionId).ToHashSet();
        var q2Ids = res2.Value!.Select(q => q.QuestionId).ToHashSet();

        if (q1Ids.Count != 2 || q2Ids.Count != 2)
        {
            allZeroOverlap = false;
            break;
        }

        var overlap = q1Ids.Intersect(q2Ids).Count();
        if (overlap > 0)
        {
            allZeroOverlap = false;
            break;
        }
    }
    Assert(allZeroOverlap, "50 lần chạy liên tiếp: Slot 1 và Slot 2 tuyệt đối 0% trùng câu hỏi khi ngân hàng đủ (>= 2 * count)");
}

// ---------------------------------------------------------------------------------
// TEST 3: Thuật toán khi ngân hàng ít câu (ít hơn 2 * MainQuestionCount) -> Trùng tối thiểu
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 3: THIẾU CÂU HỎI -> TRÙNG LẶP TỐI THIỂU ---");
{
    var (db, provider) = await SetupAsync("Db_Test3");
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "PRN222", Name = "Lập trình .NET" };
    var room = new Room { Name = "B202", Building = "Tòa B", IsActive = true };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);

    // Ngân hàng CHỈ CÓ 3 câu, nhưng mỗi slot cần 2 câu (cần 2*2 = 4 để không trùng)
    for (int i = 1; i <= 3; i++)
    {
        db.Set<Question>().Add(new Question
        {
            SubjectId = sub.Id,
            Content = $"Câu khan hiếm số {i}?",
            BloomLevel = i,
            IsActive = true
        });
    }

    var student1 = new Account { Username = "st1", PasswordHash = "hash", FullName = "Học sinh 1" };
    var student2 = new Account { Username = "st2", PasswordHash = "hash", FullName = "Học sinh 2" };
    db.Set<Account>().AddRange(student1, student2);

    var session = new ExamSession
    {
        Title = "Vấn đáp ít đề",
        SubjectId = sub.Id,
        RoomId = room.Id,
        MainQuestionCount = 2,
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddHours(1)
    };
    db.Set<ExamSession>().Add(session);
    await db.SaveChangesAsync();

    var slot1 = new ExamSlot { SessionId = session.Id, StudentId = student1.Id, OrderNo = 1, StartTime = session.StartTime, EndTime = session.StartTime.AddMinutes(15) };
    var slot2 = new ExamSlot { SessionId = session.Id, StudentId = student2.Id, OrderNo = 2, StartTime = session.StartTime.AddMinutes(15), EndTime = session.StartTime.AddMinutes(30) };
    db.Set<ExamSlot>().AddRange(slot1, slot2);
    await db.SaveChangesAsync();

    var res1 = await picker.PickForSlotAsync(slot1.Id);
    var res2 = await picker.PickForSlotAsync(slot2.Id);

    var q1Ids = res1.Value!.Select(q => q.QuestionId).ToHashSet();
    var q2Ids = res2.Value!.Select(q => q.QuestionId).ToHashSet();

    var overlap = q1Ids.Intersect(q2Ids).Count();
    // Vì tổng chỉ có 3 câu, slot 1 lấy 2, slot 2 cần 2 thì BẮT BUỘC phải lấy câu thứ 3 + đúng 1 câu từ slot 1 (overlap = 1)
    Assert(overlap == 1, "Khi chỉ có 3 câu và mỗi slot 2 câu: độ trùng lặp đúng bằng 1 (mức tối thiểu)");
}

// ---------------------------------------------------------------------------------
// TEST 4: Bốc đồng loạt cho cả phiên thi (PickForAllSlotsInSessionAsync)
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 4: BỐC ĐỒNG LOẠT TOÀN BỘ PHIÊN THI ---");
{
    var (db, provider) = await SetupAsync("Db_Test4");
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "PRN222", Name = "Lập trình .NET" };
    var room = new Room { Name = "C303", Building = "Tòa C", IsActive = true };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);

    // 15 câu hỏi trong ngân hàng
    for (int i = 1; i <= 15; i++)
    {
        db.Set<Question>().Add(new Question { SubjectId = sub.Id, Content = $"Câu ngân hàng {i}", BloomLevel = (i % 4) + 1, IsActive = true });
    }

    var session = new ExamSession { Title = "Phiên 5 slot", SubjectId = sub.Id, RoomId = room.Id, MainQuestionCount = 2, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2) };
    db.Set<ExamSession>().Add(session);
    await db.SaveChangesAsync();

    var slots = new List<ExamSlot>();
    for (int i = 1; i <= 5; i++)
    {
        var stu = new Account { Username = $"stu{i}", PasswordHash = "hash", FullName = $"Thí sinh {i}" };
        db.Set<Account>().Add(stu);
        await db.SaveChangesAsync();

        var s = new ExamSlot
        {
            SessionId = session.Id,
            StudentId = stu.Id,
            OrderNo = i,
            StartTime = session.StartTime.AddMinutes((i - 1) * 15),
            EndTime = session.StartTime.AddMinutes(i * 15)
        };
        db.Set<ExamSlot>().Add(s);
        slots.Add(s);
    }
    await db.SaveChangesAsync();

    var allResult = await picker.PickForAllSlotsInSessionAsync(session.Id);
    Assert(allResult.IsSuccess && allResult.Value!.Count == 5, "Bốc đề đồng loạt thành công cho 5 slot");

    var noAdjacentOverlap = true;
    for (int i = 0; i < 4; i++)
    {
        var curQ = allResult.Value![slots[i].Id].Select(q => q.QuestionId).ToHashSet();
        var nextQ = allResult.Value![slots[i + 1].Id].Select(q => q.QuestionId).ToHashSet();

        if (curQ.Intersect(nextQ).Any())
        {
            noAdjacentOverlap = false;
            break;
        }
    }
    Assert(noAdjacentOverlap, "Tất cả các cặp slot liên tiếp (1-2, 2-3, 3-4, 4-5) đều 0% trùng lặp câu hỏi");
}

// ---------------------------------------------------------------------------------
// TEST 5: Chặn xóa câu hỏi đã được dùng trong slot thi
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 5: BẢO VỆ DỮ LIỆU ĐÃ DÙNG ---");
{
    var (db, provider) = await SetupAsync("Db_Test5");
    var qService = provider.GetRequiredService<IQuestionService>();
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "PRN222", Name = "Lập trình .NET" };
    var room = new Room { Name = "D404", Building = "Tòa D", IsActive = true };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);

    var q1 = new Question { SubjectId = sub.Id, Content = "Câu hỏi quan trọng?", BloomLevel = 1, IsActive = true };
    db.Set<Question>().Add(q1);

    var stu = new Account { Username = "stuA", PasswordHash = "hash", FullName = "Thí sinh A" };
    db.Set<Account>().Add(stu);

    var session = new ExamSession { Title = "Phiên test", SubjectId = sub.Id, RoomId = room.Id, MainQuestionCount = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) };
    db.Set<ExamSession>().Add(session);
    await db.SaveChangesAsync();

    var slot = new ExamSlot { SessionId = session.Id, StudentId = stu.Id, OrderNo = 1, StartTime = session.StartTime, EndTime = session.StartTime.AddMinutes(15) };
    db.Set<ExamSlot>().Add(slot);
    await db.SaveChangesAsync();

    // Bốc câu hỏi cho slot
    await picker.PickForSlotAsync(slot.Id);

    // Thử xóa câu hỏi đã bốc -> Bị chặn
    var delResult = await qService.DeleteAsync(q1.Id);
    Assert(!delResult.IsSuccess, "Chặn xóa cứng câu hỏi đã từng được bốc cho thí sinh thi");

    // Khóa câu hỏi -> Thành công
    var toggleResult = await qService.ToggleActiveAsync(q1.Id);
    Assert(toggleResult.IsSuccess && !toggleResult.Value!.IsActive, "Khóa câu hỏi (IsActive = false) thành công");
}

// ---------------------------------------------------------------------------------
// TEST 6: Stress Test & Mô phỏng ngẫu nhiên Monte Carlo (100 lượt bốc)
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 6: MÔ PHỎNG MONTE CARLO (100 LƯỢT THỬ NGHIỆM ĐỘ TRÙNG LẶP) ---");
{
    var (db, provider) = await SetupAsync("Db_Test6_MonteCarlo");
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "SE1234", Name = "Kiểm thử phần mềm" };
    var room = new Room { Name = "Alpha", Building = "Tòa A", IsActive = true };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);

    // Ngân hàng 12 câu hỏi
    for (int i = 1; i <= 12; i++)
    {
        db.Set<Question>().Add(new Question
        {
            SubjectId = sub.Id,
            Content = $"Câu hỏi kiểm thử Monte Carlo số #{i}?",
            BloomLevel = (i % 4) + 1,
            IsActive = true
        });
    }
    await db.SaveChangesAsync();

    var stu1 = new Account { Username = "stuM1", PasswordHash = "hash", FullName = "Monte Carlo 1" };
    var stu2 = new Account { Username = "stuM2", PasswordHash = "hash", FullName = "Monte Carlo 2" };
    db.Set<Account>().AddRange(stu1, stu2);
    await db.SaveChangesAsync();

    var collisionCount = 0;
    var iterations = 100;

    for (int run = 1; run <= iterations; run++)
    {
        var session = new ExamSession
        {
            Title = $"Phiên Monte Carlo #{run}",
            SubjectId = sub.Id,
            RoomId = room.Id,
            MainQuestionCount = 4, // Yêu cầu 4 câu, pool có 12 câu (>= 2 * 4)
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        db.Set<ExamSession>().Add(session);
        await db.SaveChangesAsync();

        var s1 = new ExamSlot { SessionId = session.Id, StudentId = stu1.Id, OrderNo = 1, StartTime = session.StartTime, EndTime = session.StartTime.AddMinutes(15) };
        var s2 = new ExamSlot { SessionId = session.Id, StudentId = stu2.Id, OrderNo = 2, StartTime = session.StartTime.AddMinutes(15), EndTime = session.StartTime.AddMinutes(30) };
        db.Set<ExamSlot>().AddRange(s1, s2);
        await db.SaveChangesAsync();

        var res1 = await picker.PickForSlotAsync(s1.Id);
        var res2 = await picker.PickForSlotAsync(s2.Id);

        var q1Ids = res1.Value!.Select(q => q.QuestionId).ToHashSet();
        var q2Ids = res2.Value!.Select(q => q.QuestionId).ToHashSet();

        if (q1Ids.Intersect(q2Ids).Any())
        {
            collisionCount++;
        }
    }

    Assert(collisionCount == 0, $"Mô phỏng Monte Carlo 100 lần: 0/{iterations} lần trùng câu hỏi giữa 2 thí sinh liền kề");
}

// ---------------------------------------------------------------------------------
// TEST 7: Kiểm tra biên Bloom Level (1-4) và trùng lặp cùng môn vs khác môn
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 7: KIỂM TRA BIÊN BLOOM & DUPLICATE LOGIC ---");
{
    var (db, provider) = await SetupAsync("Db_Test7_Validation");
    var qService = provider.GetRequiredService<IQuestionService>();

    var subA = new Subject { Code = "IT308", Name = "Cấu trúc dữ liệu" };
    var subB = new Subject { Code = "MA101", Name = "Giải tích" };
    db.Set<Subject>().AddRange(subA, subB);
    await db.SaveChangesAsync();

    // 1. Bloom level 0 -> Thất bại
    var b0 = await qService.CreateAsync(new QuestionSaveDto { SubjectId = subA.Id, Content = "Bloom 0 test", BloomLevel = 0 });
    Assert(!b0.IsSuccess, "Từ chối câu hỏi có BloomLevel = 0 (ngoài dải 1-4)");

    // 2. Bloom level 5 -> Thất bại
    var b5 = await qService.CreateAsync(new QuestionSaveDto { SubjectId = subA.Id, Content = "Bloom 5 test", BloomLevel = 5 });
    Assert(!b5.IsSuccess, "Từ chối câu hỏi có BloomLevel = 5 (ngoài dải 1-4)");

    // 3. Bloom level 1..4 hợp lệ
    var bValid = await qService.CreateAsync(new QuestionSaveDto { SubjectId = subA.Id, Content = "Câu hỏi chuẩn môn A", BloomLevel = 3 });
    Assert(bValid.IsSuccess, "Chấp nhận câu hỏi có BloomLevel = 3 hợp lệ");

    // 4. Trùng nội dung cùng môn A -> Thất bại
    var dupSame = await qService.CreateAsync(new QuestionSaveDto { SubjectId = subA.Id, Content = "Câu hỏi chuẩn môn A", BloomLevel = 2 });
    Assert(!dupSame.IsSuccess, "Từ chối câu hỏi trùng nội dung trong cùng môn học");

    // 5. Trùng nội dung nhưng khác môn B -> Chấp nhận
    var dupDiff = await qService.CreateAsync(new QuestionSaveDto { SubjectId = subB.Id, Content = "Câu hỏi chuẩn môn A", BloomLevel = 2 });
    Assert(dupDiff.IsSuccess, "Chấp nhận câu hỏi trùng nội dung nhưng thuộc 2 môn học khác nhau");
}

// ---------------------------------------------------------------------------------
// TEST 8: Thống kê phân bố Bloom Level và tổng số câu hỏi theo môn
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 8: THỐNG KÊ PHÂN BỐ BLOOM LEVEL ---");
{
    var (db, provider) = await SetupAsync("Db_Test8_Stats");
    var qService = provider.GetRequiredService<IQuestionService>();

    var sub = new Subject { Code = "STAT101", Name = "Xác suất thống kê" };
    db.Set<Subject>().Add(sub);
    await db.SaveChangesAsync();

    // Thêm 1 câu Bloom 1, 2 câu Bloom 2, 3 câu Bloom 3, 4 câu Bloom 4
    for (int lvl = 1; lvl <= 4; lvl++)
    {
        for (int i = 1; i <= lvl; i++)
        {
            await qService.CreateAsync(new QuestionSaveDto
            {
                SubjectId = sub.Id,
                Content = $"Câu hỏi môn STAT mức Bloom {lvl} lần #{i}",
                BloomLevel = lvl
            });
        }
    }

    var stats = await qService.GetStatsAsync(sub.Id);
    Assert(stats.TotalQuestions == 10, "Thống kê tổng số 10 câu hỏi môn STAT101 chính xác");
    Assert(stats.BloomLevel1Count == 1, "Thống kê Bloom 1 = 1 câu chính xác");
    Assert(stats.BloomLevel2Count == 2, "Thống kê Bloom 2 = 2 câu chính xác");
    Assert(stats.BloomLevel3Count == 3, "Thống kê Bloom 3 = 3 câu chính xác");
    Assert(stats.BloomLevel4Count == 4, "Thống kê Bloom 4 = 4 câu chính xác");
}

// ---------------------------------------------------------------------------------
// TEST 9: Phân trang và bộ lọc tìm kiếm câu hỏi
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 9: PHÂN TRANG VÀ BỘ LỌC TÌM KIẾM ---");
{
    var (db, provider) = await SetupAsync("Db_Test9_Paging");
    var qService = provider.GetRequiredService<IQuestionService>();

    var subA = new Subject { Code = "SUB_A", Name = "Môn A" };
    var subB = new Subject { Code = "SUB_B", Name = "Môn B" };
    db.Set<Subject>().AddRange(subA, subB);
    await db.SaveChangesAsync();

    for (int i = 1; i <= 25; i++)
    {
        await qService.CreateAsync(new QuestionSaveDto
        {
            SubjectId = i <= 15 ? subA.Id : subB.Id,
            Content = $"Nội dung bài học kiến trúc ứng dụng số #{i:D2}",
            BloomLevel = (i % 4) + 1
        });
    }

    // Trang 1 size 10 môn A: GetPagedAsync(pageIndex, pageSize, subjectId, bloomLevel, search)
    var paged1 = await qService.GetPagedAsync(1, 10, subA.Id);
    Assert(paged1.TotalCount == 15 && paged1.Items.Count == 10 && paged1.TotalPages == 2, "Phân trang môn A trang 1 trả về 10/15 mục");

    // Trang 2 size 10 môn A
    var paged2 = await qService.GetPagedAsync(2, 10, subA.Id);
    Assert(paged2.Items.Count == 5, "Trang 2 môn A trả về đúng 5 mục còn lại");

    // Lọc theo từ khóa tìm kiếm "số #05"
    var pagedSearch = await qService.GetPagedAsync(1, 10, null, null, "số #05");
    Assert(pagedSearch.TotalCount == 1 && pagedSearch.Items[0].Content.Contains("#05"), "Lọc theo từ khóa tìm kiếm chính xác");
}

// ---------------------------------------------------------------------------------
// TEST 10: Xử lý ngoại lệ khi ngân hàng rỗng hoặc không đủ câu hỏi
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 10: XỬ LÝ LỖI THIẾU CÂU HỎI ---");
{
    var (db, provider) = await SetupAsync("Db_Test10_EmptyBank");
    var picker = provider.GetRequiredService<IQuestionPickerService>();

    var sub = new Subject { Code = "EMPTY1", Name = "Môn chưa có câu hỏi" };
    var room = new Room { Name = "E101", Building = "Tòa E", IsActive = true };
    var stu = new Account { Username = "stuEmpty", PasswordHash = "hash", FullName = "Học sinh E" };
    db.Set<Subject>().Add(sub);
    db.Set<Room>().Add(room);
    db.Set<Account>().Add(stu);
    await db.SaveChangesAsync();

    var session = new ExamSession
    {
        Title = "Phiên rỗng",
        SubjectId = sub.Id,
        RoomId = room.Id,
        MainQuestionCount = 3,
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddHours(1)
    };
    db.Set<ExamSession>().Add(session);
    await db.SaveChangesAsync();

    var slot = new ExamSlot
    {
        SessionId = session.Id,
        StudentId = stu.Id,
        OrderNo = 1,
        StartTime = session.StartTime,
        EndTime = session.StartTime.AddMinutes(15)
    };
    db.Set<ExamSlot>().Add(slot);
    await db.SaveChangesAsync();

    // Ngân hàng hoàn toàn rỗng -> Trả về lỗi thân thiện
    var pickEmpty = await picker.PickForSlotAsync(slot.Id);
    Assert(!pickEmpty.IsSuccess, "Báo lỗi rõ ràng khi ngân hàng môn học chưa có câu hỏi nào kích hoạt");
}

// ---------------------------------------------------------------------------------
// TEST 11: Cập nhật nội dung câu hỏi (Edit) và kiểm tra bảo toàn trạng thái
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 11: CẬP NHẬT NỘI DUNG CÂU HỎI ---");
{
    var (db, provider) = await SetupAsync("Db_Test11_Edit");
    var qService = provider.GetRequiredService<IQuestionService>();

    var sub = new Subject { Code = "EDIT1", Name = "Môn kiểm thử Edit" };
    db.Set<Subject>().Add(sub);
    await db.SaveChangesAsync();

    var created = await qService.CreateAsync(new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "Nội dung ban đầu của câu hỏi",
        BloomLevel = 1
    });

    var editResult = await qService.UpdateAsync(created.Value!.Id, new QuestionSaveDto
    {
        SubjectId = sub.Id,
        Content = "Nội dung đã được hiệu chỉnh nâng cao",
        BloomLevel = 3
    });

    Assert(editResult.IsSuccess && editResult.Value!.Content == "Nội dung đã được hiệu chỉnh nâng cao" && editResult.Value.BloomLevel == 3,
        "Cập nhật nội dung và mức độ Bloom của câu hỏi thành công");
}

// ---------------------------------------------------------------------------------
// TEST 12: Kiểm tra chuẩn hóa thang đo BloomDescriptor (Level 1..4)
// ---------------------------------------------------------------------------------
Console.WriteLine("\n--- TEST SUITE 12: CHUẨN HÓA THANG ĐO BLOOM DESCRIPTOR ---");
{
    var levels = asg2.BLL.Common.BloomDescriptor.GetAllLevels();
    Assert(levels.Count == 4, "BloomDescriptor có đủ 4 cấp độ tư duy chuẩn");

    var l1 = asg2.BLL.Common.BloomDescriptor.GetDetail(1);
    Assert(l1.VietnameseName == "Nhận biết" && l1.ActionKeywords.Contains("Liệt kê"), "Mức 1 nhận biết chuẩn từ khóa hành động");

    var l2 = asg2.BLL.Common.BloomDescriptor.GetDetail(2);
    Assert(l2.VietnameseName == "Thông hiểu" && l2.BadgeClass.Contains("bg-info"), "Mức 2 thông hiểu chuẩn badge css class");

    var l3 = asg2.BLL.Common.BloomDescriptor.GetDetail(3);
    Assert(l3.VietnameseName == "Vận dụng" && l3.ActionKeywords.Contains("Viết mã nguồn"), "Mức 3 vận dụng chuẩn từ khóa code");

    var l4 = asg2.BLL.Common.BloomDescriptor.GetDetail(4);
    Assert(l4.VietnameseName == "Phân tích & Đánh giá" && l4.BadgeClass.Contains("bg-danger"), "Mức 4 phân tích đánh giá chuẩn badge danger");
}

Console.WriteLine("\n======================================================================");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($" TỔNG KẾT: {passed}/{total} BÀI KIỂM THỬ ĐẠT CHUẨN (100% SUCCESS) ");
Console.ResetColor();
Console.WriteLine("======================================================================");


