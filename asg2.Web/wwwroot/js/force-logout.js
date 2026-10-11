// Tài khoản bị khóa/xóa thì server gửi ForceLogout: đưa về trang đăng nhập kèm thông báo.
(function () {
    if (!window.ExamEvents) { return; }

    ExamEvents.on('ForceLogout', function () {
        window.location.href = '/Account/Login?reason=locked';
    });
})();
