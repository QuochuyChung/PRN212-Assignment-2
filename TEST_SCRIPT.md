# Kịch bản test thủ công — Asg2 (Quản lý kỳ thi và lịch thi vấn đáp)

Bản này viết dựa trên code trên `main` (commit `fd3193a`) và dữ liệu mẫu do seeder tạo. Thông báo lỗi trong cột "Kết quả mong đợi" là chuỗi thật trong code; chỉ cần khớp ý, không cần khớp từng dấu.

## 0. Chuẩn bị

| Việc | Chi tiết |
|---|---|
| Địa chỉ | http://localhost:5010 |
| Tài khoản | Admin: `admin`. Sinh viên: `student1` … `student10`. Mật khẩu mẫu xem [README.md](README.md) mục 2 |
| **Cửa sổ A** | Trình duyệt thường, đăng nhập **admin** |
| **Cửa sổ B** | Cửa sổ **ẩn danh** (hoặc trình duyệt khác), đăng nhập **student1**. Phải ẩn danh vì cookie dùng chung giữa các tab thường |
| **Cửa sổ C** | Không phải tài khoản mới: chỉ là **tab thứ hai trong cửa sổ A**, cùng đăng nhập `admin` (các tab thường dùng chung cookie). Dùng để "một tab thao tác, một tab nhìn tự đổi" ở các mục có nhắc cửa sổ C |
| Nên mở | F12 → Console ở cả hai cửa sổ, để thấy lỗi JS (không được có lỗi đỏ) |

**Dữ liệu mẫu có sẵn:**
- 6 môn: IT308, MA101, MO201, PRN222, SE1234, WD192.
- 5 phòng: A101, A102 (Tòa A), B203, B204 (Tòa B), C305 (Tòa C).
- 10 sinh viên, 60 câu hỏi (10 câu mỗi môn).
- 3 phiên thi:
  - `PRN222 - Ca sáng`: phòng A101, 14/10/2026 15:00–18:00, 15 phút/slot (sức chứa 12), 2 câu chính, **đã có sẵn 2 thí sinh**.
  - `SE1234 - Ca chiều`: phòng B203, 14/10/2026 20:30–23:30, 20 phút/slot (sức chứa 9), 2 câu chính, chưa có thí sinh.
  - `IT308 - Cơ sở dữ liệu`: phòng C305, 15/10/2026 16:00–18:30, 15 phút/slot (sức chứa 10), 3 câu chính, chưa có thí sinh.

**Quy ước ghi kết quả:** ✅ đạt · ❌ lỗi (ghi cách tái hiện) · ⏭ bỏ qua. Điền vào bảng ở mục 11.

---

## 1. Đăng nhập và phân quyền (P1)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 1.1 | Mở `/Account/Login`, bấm Đăng nhập khi để trống | Báo "Vui lòng nhập tên đăng nhập / mật khẩu", chưa gửi lên server |
| 1.2 | Nhập đúng `admin`, sai mật khẩu | "Sai tên đăng nhập hoặc mật khẩu." |
| 1.3 | Nhập username không tồn tại | Cùng thông báo ở 1.2 (không lộ user nào tồn tại) |
| 1.4 | Đăng nhập `admin` đúng | Vào `/Admin/Accounts`; menu có: Tài khoản, Dashboard, Môn học, Phòng thi, Phiên thi, Gán sinh viên, Ngân hàng câu hỏi; góc phải hiện tên + "Admin" |
| 1.5 | Đăng nhập `student1` đúng | Vào `/Student`; menu chỉ có Trang chủ, Lịch thi chung, Lịch của tôi |
| 1.6 | Khi là student1, gõ tay `/Admin/Sessions`, `/Admin/Accounts`, `/Admin/Questions` | Chuyển tới trang "Không có quyền truy cập" |
| 1.7 | Chưa đăng nhập, gõ `/Admin/Dashboard` và `/Student/MySlots` | Chuyển về trang đăng nhập |
| 1.8 | Bấm Đăng xuất | Về trang đăng nhập; bấm Back của trình duyệt rồi F5 vẫn bị đẩy ra login |
| 1.9 | Bấm biểu tượng con mắt ở ô mật khẩu | Hiện/ẩn mật khẩu |
| 1.10 | Đã đăng nhập mà vào lại `/Account/Login` | Tự chuyển tới trang chính của role |

## 2. Tài khoản sinh viên (P1) — dùng cửa sổ A (admin)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 2.1 | Vào **Tài khoản** | 10 sinh viên, trạng thái "Hoạt động"; không thấy tài khoản admin |
| 2.2 | Ô tìm: gõ `student3` rồi Tìm | Còn 1 dòng; bấm "Xóa lọc" thì trở lại 10 dòng |
| 2.3 | **+ Thêm tài khoản**: `test01` / `Sinh Viên Test` / mật khẩu `abc123` / tick Hoạt động | Về danh sách, toast "Đã tạo tài khoản test01", dòng mới xuất hiện |
| 2.4 | Thêm lại username `student1` | Lỗi "Tên đăng nhập đã tồn tại." |
| 2.5 | Thêm username `ab` (quá ngắn) hoặc `a b!` (ký tự lạ) | Lỗi "Tên đăng nhập 3-50 ký tự, chỉ gồm chữ, số, dấu _ . -" |
| 2.6 | Thêm với mật khẩu `123` | Lỗi "Mật khẩu từ 6 đến 100 ký tự." |
| 2.7 | Thêm với họ tên để trống | Lỗi "Họ tên là bắt buộc." |
| 2.8 | **Sửa** `test01`: đổi họ tên, để trống mật khẩu, Lưu | Cập nhật tên; mật khẩu cũ `abc123` vẫn đăng nhập được |
| 2.9 | **Sửa** `test01`: nhập mật khẩu mới `xyz789`, Lưu | Đăng nhập (cửa sổ B) bằng mật khẩu mới được, mật khẩu cũ không còn |
| 2.10 | **Khóa tài khoản đang online:** cửa sổ B đăng nhập `test01` và đang mở một trang. Cửa sổ A sửa `test01`, bỏ tick "Đang hoạt động", Lưu | Cửa sổ B **tự bị đá** về trang đăng nhập kèm thông báo "Tài khoản của bạn đã bị khóa hoặc bị xóa..." (không cần F5). Dòng trong danh sách đổi sang "Đã khóa" |
| 2.11 | Cửa sổ B đăng nhập lại `test01` | "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên." |
| 2.12 | **Xóa** `test01` (nút Xóa → xác nhận) | Toast "Đã xóa tài khoản", dòng biến mất |
| 2.13 | Xóa một sinh viên **đã có slot thi** (ví dụ người đang ở phiên PRN222) | Bị từ chối: "Tài khoản đã được sử dụng (ví dụ đã có lịch thi) nên không thể xóa. Hãy khóa..." |
| 2.14 | Thêm/xóa tài khoản ở cửa sổ A khi cửa sổ C (admin) đang mở trang Tài khoản | Cửa sổ C tự thêm/xóa dòng kèm highlight vàng nhạt, không cần F5 |

## 3. Môn học (P2)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 3.1 | Vào **Môn học** | 6 môn |
| 3.2 | **+ Thêm môn học**: mã `TEST01`, tên `Môn thử nghiệm` | Tạo thành công, hiện trong danh sách |
| 3.3 | Thêm lại mã trùng `TEST01` | "Mã môn đã tồn tại." |
| 3.4 | Mã `A` (1 ký tự) hoặc `AB CD` / `AB_CD` | "Mã môn 2-20 ký tự, chỉ gồm chữ, số và dấu gạch ngang." |
| 3.5 | Tên để trống / dài hơn 100 ký tự | "Tên môn học là bắt buộc." / "Tên môn học tối đa 100 ký tự." |
| 3.6 | Sửa tên `TEST01` | Cập nhật |
| 3.7 | Xóa môn **đang dùng** (ví dụ `PRN222`) | "Môn học đang được sử dụng bởi các phiên thi nên không thể xóa." |
| 3.8 | Xóa `TEST01` (chưa dùng) | Xóa được |
| 3.9 | Tìm theo mã/tên | Lọc đúng |
| 3.10 | Thêm tên môn `<script>alert(1)</script>` | Hiển thị nguyên văn như chữ, **không** có hộp thoại alert |

## 4. Phòng thi (P2)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 4.1 | Vào **Phòng thi** | 5 phòng, tất cả "Hoạt động" |
| 4.2 | **+ Thêm phòng**: `T-101`, tòa `Tòa T` | Tạo thành công |
| 4.3 | Thêm tên trùng `A101` | "Tên phòng đã tồn tại." |
| 4.4 | Tên trống / dài hơn 50 ký tự; tòa nhà dài hơn 50 ký tự | "Tên phòng là bắt buộc." / "Tên phòng tối đa 50 ký tự." / "Tên tòa nhà tối đa 50 ký tự." |
| 4.5 | Xóa phòng **đang dùng** (`A101`, `B203` hoặc `C305`) | "Phòng thi đang được sử dụng bởi các phiên thi nên không thể xóa." |
| 4.6 | Sửa `T-101`: bỏ tick hoạt động (khóa phòng) | Trạng thái đổi |
| 4.7 | Sang **Phiên thi → Tạo** và chọn phòng `T-101` đã khóa (xem 6.9) | Bị chặn: "Phòng thi đã bị khóa, không thể xếp lịch." |
| 4.8 | Xóa `T-101` (chưa dùng) | Xóa được |

## 5. Dashboard (P2) — hai cửa sổ admin (A và C)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 5.1 | Cửa sổ C mở **Dashboard** | Số liệu tổng quan khớp dữ liệu mẫu: 6 môn, 5 phòng, 10 sinh viên, 3 phiên thi, 60 câu hỏi |
| 5.2 | Cửa sổ A thêm một phòng (hoặc môn) | Số liệu ở cửa sổ C **tự tăng**, không F5 |
| 5.3 | Cửa sổ A xóa phòng/môn vừa thêm | Số liệu ở C tự giảm |
| 5.4 | Cửa sổ A thêm một phiên thi / một câu hỏi / một tài khoản | Các số liệu tương ứng ở C đổi theo |

## 6. Phiên thi (P3) — cửa sổ A; cửa sổ B (student1) mở **Lịch thi chung**

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 6.1 | A: vào **Phiên thi** | 3 phiên mẫu; bộ lọc theo môn và trạng thái (Sắp tới / Hoàn thành / Đã hủy) |
| 6.2 | Lọc theo môn `PRN222` | Chỉ còn phiên PRN222 |
| 6.3 | **Tạo phiên thi mới** hợp lệ: tiêu đề `TEST - Ca 1`, môn `MA101`, phòng `A102`, 16/10/2026 08:00–10:00, 15 phút/slot, 2 câu chính, 1 câu phụ | Tạo thành công. **B thấy dòng mới hiện ra ngay** trên Lịch thi chung (highlight), không F5 |
| 6.4 | Tiêu đề để trống | "Tiêu đề phiên thi không được để trống." |
| 6.5 | Giờ kết thúc ≤ giờ bắt đầu | "Thời gian kết thúc phải sau thời gian bắt đầu." |
| 6.6 | Thời lượng slot `4` và `181` | "Thời lượng mỗi slot phải từ 5 đến 180 phút." |
| 6.7 | Khung 08:00–08:10 với slot 15 phút | "Khung giờ phiên thi phải đủ chứa ít nhất 1 slot." |
| 6.8 | Số câu chính `0`; số câu phụ `-1` | "Số câu hỏi chính phải từ 1 trở lên." / "Số câu hỏi phụ không được âm." |
| 6.9 | Chọn phòng đã khóa (làm 4.6 trước) | "Phòng thi đã bị khóa, không thể xếp lịch." |
| 6.10 | **Trùng phòng:** tạo phiên ở phòng `A101` ngày 14/10/2026 16:00–17:00 (chồng với PRN222 15:00–18:00) | "Phòng thi này đã có phiên thi khác trùng khung giờ." |
| 6.11 | Cùng giờ nhưng phòng khác (`A102`) | Tạo được |
| 6.12 | **Sửa** phiên `TEST - Ca 1`: đổi phòng hoặc giờ | Lưu được. **B thấy dòng cập nhật ngay** |
| 6.13 | Bấm **Chi tiết** một phiên | Xem được thông tin phiên (môn, phòng, giờ, số câu hỏi, thí sinh) |
| 6.14 | **Xóa** phiên `TEST - Ca 1` | **B thấy dòng biến mất ngay** kèm thông báo; phiên không còn trong danh sách |
| 6.15 | B (student1) bấm Lịch thi chung → các phiên | Chỉ xem, không có nút Sửa/Xóa |
| 6.16 | Đổi trạng thái một phiên sang "Đã hủy" / "Hoàn thành" | Phiên đổi trạng thái; không còn xuất hiện trong danh sách "phiên sắp tới" ở trang Gán sinh viên |

## 7. Gán sinh viên và chia slot (P4)

Tạo trước phiên thử cho mục này: `TEST - Slot`, môn `PRN222`, phòng `A102`, 16/10/2026 09:00–10:00, **15 phút/slot (sức chứa 4)**, 2 câu chính. Cửa sổ B ở trang **Lịch của tôi** (student1).

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 7.1 | A: **Gán sinh viên** → chọn phiên `TEST - Slot` → "Xem danh sách slot" | Phiên trống, chưa có slot nào |
| 7.2 | Bấm gán khi chưa tick ai | "Vui lòng chọn ít nhất một sinh viên." |
| 7.3 | Tick `student1` và `student2` → Gán | Slot #1 09:00–09:15, slot #2 09:15–09:30. **B thấy slot mới trong Lịch của tôi ngay**, không F5 |
| 7.4 | Gán lại `student1` vào cùng phiên | "Các sinh viên sau đã được gán vào phiên này: ..." |
| 7.5 | Tick 3 sinh viên mới trong khi chỉ còn 2 chỗ | "Phiên chỉ còn 2 chỗ trống (sức chứa tối đa 4 thí sinh), không đủ cho 3 sinh viên đã chọn." |
| 7.6 | Gỡ `student1` khỏi slot #1 | **B thấy slot biến mất ngay**; slot #1 trống |
| 7.7 | Gán thêm `student3` | `student3` lấp **slot #1 (trống sớm nhất)**, không phải slot #3 |
| 7.8 | **Trùng lịch giữa phiên:** tạo phiên `TEST - Clash` phòng `B204`, **14/10/2026 15:00–15:15**, slot 15 phút (sức chứa 1). Xem ở phiên PRN222 xem ai đang ở slot 15:00–15:15, rồi gán đúng người đó vào `TEST - Clash` | Bị chặn: "Sinh viên ... đã có lịch thi trùng với tất cả khung giờ còn trống." |
| 7.9 | **Đổi giờ phiên có thí sinh:** sửa `TEST - Slot` kéo giờ kết thúc về 09:15 (sức chứa còn 1) trong khi đang có 2 thí sinh | Bị từ chối: "Khung giờ mới chỉ chứa được 1 thí sinh, trong khi phiên đã có 2 thí sinh." |
| 7.10 | Gán lại `student1` vào `TEST - Slot` (B thấy slot). Rồi sửa phiên dời cả khung sang **09:30–10:30** (sức chứa vẫn 4) | Lưu được; giờ slot của từng thí sinh dời theo 30 phút; **B thấy giờ trong Lịch của tôi cập nhật** ngay |
| 7.11 | Đổi `TEST - Slot` sang "Hoàn thành" hoặc "Đã hủy", vào lại Gán sinh viên | Phiên không còn trong danh sách chọn |
| 7.12 | Xóa phiên `TEST - Slot` khi `student1` vẫn còn slot trong đó | B thấy slot biến mất ngay; phiên không còn trong danh sách |

## 8. Ngân hàng câu hỏi và bốc đề (P5)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 8.1 | A: vào **Ngân hàng câu hỏi** | Tổng 60 câu, thống kê theo mức Bloom 1–4, phân trang (6 trang) |
| 8.2 | Lọc theo môn `PRN222`; lọc theo "Mức 3"; gõ từ khóa | Danh sách và số kết quả đổi đúng |
| 8.3 | **Thêm câu hỏi mới**: môn `PRN222`, Bloom 2, nội dung `Câu hỏi thử số 1`, tick kích hoạt | Tạo được; cửa sổ admin khác (C) thấy dòng mới và tổng số tăng |
| 8.4 | Thêm lại đúng nội dung đó cho cùng môn | "Câu hỏi với nội dung này đã tồn tại trong môn học đã chọn." |
| 8.5 | Nội dung để trống / dài hơn 2000 ký tự | "Nội dung câu hỏi không được để trống." / "...không được vượt quá 2000 ký tự." |
| 8.6 | Chọn Bloom ngoài 1–4 (nếu sửa được HTML) | "Mức độ Bloom phải nằm trong khoảng từ 1 ... đến 4 ..." |
| 8.7 | Sửa nội dung/Bloom của câu `Câu hỏi thử số 1` | Cập nhật, C thấy đổi ngay |
| 8.8 | Khóa câu hỏi (bỏ tick kích hoạt) | Trạng thái đổi; câu bị khóa không được bốc cho ca thi mới |
| 8.9 | Xóa câu hỏi **chưa từng được bốc** | Xóa được; C thấy dòng biến mất |
| 8.10 | **Bốc đề tự động:** gán `student1`, `student2` vào phiên mới (2 câu chính). B (student1) vào **Lịch của tôi** → **Xem đề** | Hiện đúng 2 câu hỏi, có mức Bloom |
| 8.11 | Admin ở trang Gán sinh viên xem slot #1 và slot #2 | Hai slot có bộ câu hỏi **khác nhau hoàn toàn** (không trùng với thí sinh liền trước) |
| 8.12 | Admin bấm **Bốc lại đề** cho slot #1 | **B đổi sang bộ câu hỏi mới ngay**, có hiệu ứng highlight, không F5 |
| 8.13 | Xóa một câu hỏi **đã được bốc cho thí sinh** | Bị từ chối: "Câu hỏi đã từng được bốc cho thí sinh dự thi. Hãy khóa câu hỏi thay vì xóa..." |
| 8.14 | Câu hỏi chứa `<b>in đậm</b>` hoặc `<script>` | Hiển thị nguyên văn như chữ, ở cả trang Admin và trang đề của sinh viên |

## 9. Realtime và kết nối (xuyên suốt)

| ID | Bước | Kết quả mong đợi |
|---|---|---|
| 9.1 | Hai cửa sổ admin (A, C), A thêm/sửa/xóa môn, phòng, tài khoản, câu hỏi | C luôn tự cập nhật, console không có lỗi đỏ |
| 9.2 | Cửa sổ B (student) mở Lịch thi chung; A thêm/sửa/xóa phiên thi | B cập nhật ngay, Console không có lỗi |
| 9.3 | Hai sinh viên B1 (student1) và B2 (student2) ở hai cửa sổ ẩn danh; admin gán slot cho student1 | **Chỉ** B1 nhận slot mới trong Lịch của tôi; B2 không |
| 9.4 | Console của một cửa sổ **chưa đăng nhập**: `fetch('/hubs/exam/negotiate?negotiateVersion=1',{method:'POST'}).then(r=>r.text()).then(console.log)` | Không trả JSON có `connectionToken` (trả 401 hoặc HTML trang đăng nhập) |
| 9.5 | **Mất kết nối rồi nối lại:** dừng app (Ctrl+C) vài giây rồi chạy lại. Trang đang mở sẽ tự thử kết nối lại | Sau khi app lên, trang tự nhận lại sự kiện và tải lại dữ liệu; nếu quá lâu thì F5 là hết |

## 10. Kiến trúc (để trả lời khi thầy hỏi)

| ID | Việc | Kết quả mong đợi |
|---|---|---|
| 10.1 | `powershell -NoProfile -File tools/check-layers.ps1` | In `OK: tang Web khong cham DAL` |
| 10.2 | `dotnet run --project tools/test-question-picker` | 10 ca kiểm thử bốc đề đều đạt |
| 10.3 | `dotnet build asg2.sln` | 0 lỗi, 0 cảnh báo |
| 10.4 | Chứng minh khóa kỹ thuật: tạm thêm dòng `using asg2.DAL.Entities;` vào một file `.cs` trong `asg2.Web`, rồi build | Lỗi biên dịch `CS0234 ... 'DAL' does not exist in the namespace 'asg2'`. **Xóa dòng đó đi** sau khi thử |
| 10.5 | Mở `asg2.Web/asg2.Web.csproj` và `asg2.BLL/asg2.BLL.csproj` | Web chỉ tham chiếu BLL; BLL tham chiếu DAL với `PrivateAssets="compile"` |
| 10.6 | Một PageModel bất kỳ (ví dụ `Pages/Admin/Rooms/Index.cshtml.cs`) | Chỉ inject service ở BLL (`IRoomService`), không có DbContext/Repository |
| 10.7 | Connection string | Chỉ nằm trong `appsettings.json` (không hardcode trong code) |

## 11. Bảng ghi kết quả

| Mục | Số ca | ✅ | ❌ | ⏭ | Ghi chú (ID lỗi, cách tái hiện) |
|---|---|---|---|---|---|
| 1. Đăng nhập, phân quyền | 10 | | | | |
| 2. Tài khoản | 14 | | | | |
| 3. Môn học | 10 | | | | |
| 4. Phòng thi | 8 | | | | |
| 5. Dashboard | 4 | | | | |
| 6. Phiên thi | 16 | | | | |
| 7. Gán sinh viên, slot | 12 | | | | |
| 8. Câu hỏi, bốc đề | 14 | | | | |
| 9. Realtime, kết nối | 5 | | | | |
| 10. Kiến trúc | 7 | | | | |

**Khi gặp lỗi, ghi:** ID ca, tài khoản đang dùng, trang nào, bấm gì, thông báo/ảnh chụp màn hình và dòng đỏ ở Console (F12) nếu có. Gửi cho người phụ trách mục tương ứng: tài khoản = P1, môn/phòng/dashboard/SignalR = P2, phiên thi = P3, gán slot = P4, câu hỏi/bốc đề = P5.

## 12. Dọn dữ liệu thử sau khi test

Xóa các bản ghi tên `TEST ...`, `T-101`, `TEST01`, `test01`, `Câu hỏi thử số 1` để không làm bẩn DB dùng chung. Nếu test trên DB riêng (LocalDB) thì bỏ qua.
