// Dashboard: tải lại số liệu qua JSON handler (?handler=Stats) mỗi khi admin ở trang này
// nhận BẤT KỲ sự kiện realtime nào (P3–P5 thêm sự kiện mới thì thêm tên vào EVENTS).
(function () {
    var container = document.getElementById('dashboard-stats');
    if (!container || !window.ExamEvents) { return; }

    // Đồng bộ với EVENTS trong exam-hub.js + pseudo-event ExamHubReconnected.
    var EVENTS = [
        'AccountCreated', 'AccountUpdated', 'AccountDeleted', 'ForceLogout',
        'SubjectCreated', 'SubjectUpdated', 'SubjectDeleted',
        'RoomCreated', 'RoomUpdated', 'RoomDeleted',
        'SessionCreated', 'SessionUpdated', 'SessionDeleted',
        'SlotAssigned', 'SlotRemoved',
        'QuestionCreated', 'QuestionUpdated', 'QuestionDeleted',
        'SlotQuestionsUpdated',
        'ExamHubReconnected'
    ];

    var inflight = false;

    function setText(id, value) {
        var el = document.getElementById(id);
        if (el) { el.textContent = String(value); }
    }

    function applyStats(stats) {
        setText('stat-total-subjects', stats.totalSubjects);
        setText('stat-total-rooms', stats.totalRooms);
        setText('stat-active-rooms', stats.activeRooms);
        setText('stat-total-students', stats.totalStudents);
        setText('stat-total-sessions', stats.totalSessions);
        setText('stat-total-questions', stats.totalQuestions);
    }

    function refreshStats() {
        // Nhiều sự kiện liên tiếp gộp làm một request.
        if (inflight) { return; }
        inflight = true;

        fetch('/Admin/Dashboard?handler=Stats', { headers: { 'Accept': 'application/json' } })
            .then(function (res) {
                if (!res.ok) { throw new Error('HTTP ' + res.status); }
                return res.json();
            })
            .then(applyStats)
            .catch(function (err) {
                console.debug('dashboard.js: không tải lại được số liệu.', err);
            })
            .finally(function () {
                inflight = false;
            });
    }

    EVENTS.forEach(function (name) {
        ExamEvents.on(name, refreshStats);
    });
})();
