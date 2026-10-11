// =====================================================================================
// exam-hub.js — bus sự kiện realtime DÙNG CHUNG cho mọi trang (P2 thay bản stub của P1).
//
// API mà mọi trang dùng (giữ nguyên, KHÔNG đổi):
//   ExamEvents.on("SessionCreated", function (dto) { ... });   // đăng ký handler
//   ExamEvents.off("SessionCreated", handler);                 // bỏ đăng ký
//   ExamEvents.emit(name, ...args);                            // gọi khi nhận event
//
// Quy ước: tên event lấy từ HubEvents (asg2.BLL/Realtime/HubEvents.cs); payload JSON camelCase.
// Trang nào cũng chỉ đăng ký handler, KHÔNG tự tạo connection SignalR mới.
// Pseudo-event (không do server gửi): "ExamHubReconnected" — trang nào muốn tải lại dữ liệu
// sau khi kết nối lại thì ExamEvents.on("ExamHubReconnected", reload).
// =====================================================================================
window.ExamEvents = (function () {
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

(function () {
    // Đồng bộ với HubEvents (server cũng chỉ bắn đúng những tên này).
    var EVENTS = [
        'AccountCreated', 'AccountUpdated', 'AccountDeleted', 'ForceLogout',
        'SubjectCreated', 'SubjectUpdated', 'SubjectDeleted',
        'RoomCreated', 'RoomUpdated', 'RoomDeleted',
        'SessionCreated', 'SessionUpdated', 'SessionDeleted',
        'SlotAssigned', 'SlotRemoved',
        'QuestionCreated', 'QuestionUpdated', 'QuestionDeleted',
        'SlotQuestionsUpdated'
    ];

    if (typeof signalR === 'undefined') {
        console.error('exam-hub.js: chưa tải thư viện SignalR (lib/signalr/dist/browser/signalr.min.js).');
        return;
    }

    // MỘT connection duy nhất cho cả ứng dụng; mất kết nối thì tự nối lại (withAutomaticReconnect).
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/exam')
        .withAutomaticReconnect()
        .build();

    EVENTS.forEach(function (name) {
        connection.on(name, function (payload) {
            window.ExamEvents.emit(name, payload);
        });
    });

    connection.onreconnected(function () {
        // Sau khi nối lại, dữ liệu trên trang có thể đã cũ: báo cho trang tự tải lại.
        window.ExamEvents.emit('ExamHubReconnected');
    });

    connection.onclose(function () {
        console.warn('exam-hub.js: mất kết nối SignalR, thử lại...');
        startWithRetry();
    });

    var attempts = 0;
    var everConnected = false;

    function startWithRetry() {
        attempts++;
        connection.start()
            .then(function () {
                attempts = 0;
                // Nối lại sau khi rớt: cũng phải báo trang tải lại (onreconnected chỉ chạy với reconnect tự động).
                if (everConnected) {
                    window.ExamEvents.emit('ExamHubReconnected');
                }
                everConnected = true;
            })
            .catch(function (err) {
                // Chưa đăng nhập (401) hoặc server chưa lên: thử lại vài lần rồi thôi.
                if (attempts >= 3) {
                    console.debug('exam-hub.js: không kết nối được /hubs/exam.', err);
                    return;
                }
                console.debug('exam-hub.js: kết nối thất bại, thử lại sau.', err);
                setTimeout(startWithRetry, 3000 * attempts);
            });
    }

    startWithRetry();
})();
