# Kiểm thử tự động P4 - Gán sinh viên vào phiên thi

Chạy từ thư mục gốc của solution:

```powershell
dotnet run --project tools/test-slot-assignment
```

Bộ kiểm thử dùng EF Core InMemory và các repository/service thật của ứng dụng. Các ca kiểm thử bao phủ việc tự chia giờ, lấp lại slot trống, kiểm tra sức chứa theo kiểu atomic, chặn tài khoản không hợp lệ, ghép lịch không trùng và phát sự kiện realtime sau khi lưu thành công.
