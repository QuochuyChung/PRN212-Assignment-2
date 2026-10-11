// Tiện ích dùng chung (P1). Thông báo dạng toast: AppToast.show("nội dung", "success|danger|warning|info").
window.AppToast = (function () {
    function show(message, type) {
        var container = document.getElementById('toast-container');
        if (!container || !window.bootstrap) { return; }

        var toast = document.createElement('div');
        toast.className = 'toast align-items-center text-bg-' + (type || 'info') + ' border-0';
        toast.setAttribute('role', 'alert');

        var wrapper = document.createElement('div');
        wrapper.className = 'd-flex';

        var body = document.createElement('div');
        body.className = 'toast-body';
        body.textContent = message; // textContent: không bao giờ chèn HTML từ dữ liệu

        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close btn-close-white me-2 m-auto';
        close.setAttribute('data-bs-dismiss', 'toast');
        close.setAttribute('aria-label', 'Đóng');

        wrapper.appendChild(body);
        wrapper.appendChild(close);
        toast.appendChild(wrapper);
        container.appendChild(toast);

        toast.addEventListener('hidden.bs.toast', function () { toast.remove(); });
        new bootstrap.Toast(toast, { delay: 4000 }).show();
    }

    return { show: show };
})();

// Hiện thông báo một lần do server đặt qua TempData (xem _Layout).
document.addEventListener('DOMContentLoaded', function () {
    var flash = document.getElementById('flash');
    if (flash) {
        AppToast.show(flash.dataset.message, flash.dataset.type);
    }
});
