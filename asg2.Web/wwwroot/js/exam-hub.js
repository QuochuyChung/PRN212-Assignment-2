// =====================================================================================
// STUB do P1 tạo để các trang chạy được trước khi có SignalR.
// P2 THAY file này bằng bản kết nối SignalR thật (hub /hubs/exam) nhưng GIỮ NGUYÊN API bên dưới,
// vì mọi trang (account-list.js, force-logout.js, schedule.js, ...) chỉ dùng đúng API này:
//
//   ExamEvents.on("SessionCreated", function (dto) { ... });   // đăng ký handler
//   ExamEvents.off("SessionCreated", handler);                  // bỏ đăng ký
//   ExamEvents.emit(name, ...args);                             // P2 gọi khi connection.on(...) nhận event
//
// Quy ước: tên event lấy từ HubEvents (asg2.BLL/Realtime/HubEvents.cs); payload JSON camelCase.
// Trang nào cũng chỉ đăng ký handler, KHÔNG tự tạo connection SignalR mới.
// =====================================================================================
window.ExamEvents = window.ExamEvents || (function () {
    var handlers = {};

    return {
        on: function (name, handler) {
            (handlers[name] = handlers[name] || []).push(handler);
        },
        off: function (name, handler) {
            handlers[name] = (handlers[name] || []).filter(function (h) { return h !== handler; });
        },
        emit: function (name) {
            var args = Array.prototype.slice.call(arguments, 1);
            (handlers[name] || []).slice().forEach(function (h) { h.apply(null, args); });
        }
    };
})();
