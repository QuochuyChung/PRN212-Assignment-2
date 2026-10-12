// Lịch thi chung của sinh viên cập nhật realtime (SessionCreated / SessionUpdated / SessionDeleted)
(function () {
    var tbody = document.getElementById('schedule-body');
    var emptyNote = document.getElementById('schedule-empty');
    if (!tbody || !window.ExamEvents) { return; }

    var isFiltered = tbody.dataset.filtered === 'true';

    function findRow(id) {
        return tbody.querySelector('tr[data-id="' + id + '"]');
    }

    function cell(className) {
        var td = document.createElement('td');
        if (className) { td.className = className; }
        return td;
    }

    function formatHourOnly(isoString) {
        var d = new Date(isoString);
        var pad = function (n) { return n < 10 ? '0' + n : n; };
        return pad(d.getHours()) + ':' + pad(d.getMinutes());
    }

    function formatDateOnly(isoString) {
        var d = new Date(isoString);
        var pad = function (n) { return n < 10 ? '0' + n : n; };
        return pad(d.getDate()) + '/' + pad(d.getMonth() + 1) + '/' + d.getFullYear();
    }

    function buildRow(s) {
        var tr = document.createElement('tr');
        tr.dataset.id = s.id;

        // Title
        var tdTitle = cell('col-title ps-3 fw-bold');
        tdTitle.textContent = s.title;
        tr.appendChild(tdTitle);

        // Subject
        var tdSubject = cell('col-subject');
        var badgeSub = document.createElement('span');
        badgeSub.className = 'badge badge-soft-primary';
        badgeSub.textContent = s.subjectCode;
        tdSubject.appendChild(badgeSub);
        var divSubName = document.createElement('div');
        divSubName.className = 'small text-muted';
        divSubName.textContent = s.subjectName;
        tdSubject.appendChild(divSubName);
        tr.appendChild(tdSubject);

        // Room
        var tdRoom = cell('col-room');
        var badgeRoom = document.createElement('span');
        badgeRoom.className = 'badge badge-soft-info';
        badgeRoom.textContent = s.roomName;
        tdRoom.appendChild(badgeRoom);
        tr.appendChild(tdRoom);

        // Date
        var tdDate = cell('col-date');
        tdDate.textContent = formatDateOnly(s.startTime);
        tr.appendChild(tdDate);

        // Hours
        var tdHours = cell('col-hours');
        var badgeHours = document.createElement('span');
        badgeHours.className = 'badge badge-soft-secondary';
        badgeHours.textContent = formatHourOnly(s.startTime) + ' - ' + formatHourOnly(s.endTime);
        tdHours.appendChild(badgeHours);
        tr.appendChild(tdHours);

        // Slot
        var tdSlot = cell('col-slot small');
        tdSlot.textContent = s.slotMinutes + ' phút/thí sinh';
        tr.appendChild(tdSlot);

        // Questions
        var tdQues = cell('col-questions small');
        tdQues.textContent = s.mainQuestionCount + ' câu chính';
        tr.appendChild(tdQues);

        // Capacity
        var tdCap = cell('col-capacity pe-3');
        var badgeCap = document.createElement('span');
        badgeCap.className = 'badge ' + (s.assignedCount >= s.capacity ? 'badge-soft-warning' : 'badge-soft-secondary');
        badgeCap.textContent = s.assignedCount + ' / ' + s.capacity;
        tdCap.appendChild(badgeCap);
        tr.appendChild(tdCap);

        return tr;
    }

    function flash(row) {
        row.classList.remove('row-flash');
        void row.offsetWidth;
        row.classList.add('row-flash');
    }

    function refreshEmptyNote() {
        if (emptyNote) { emptyNote.hidden = tbody.children.length > 0; }
    }

    ExamEvents.on('SessionCreated', function (s) {
        if (s.status !== 'Scheduled' || isFiltered || findRow(s.id)) { return; }
        var row = buildRow(s);
        tbody.prepend(row);
        flash(row);
        refreshEmptyNote();
        AppToast.show('Có phiên thi mới: ' + s.title, 'info');
    });

    ExamEvents.on('SessionUpdated', function (s) {
        var row = findRow(s.id);
        if (s.status === 'Cancelled' || s.status === 'Completed') {
            if (row) {
                row.remove();
                refreshEmptyNote();
                if (s.status === 'Cancelled') {
                    AppToast.show('Phiên thi "' + s.title + '" đã bị hủy', 'warning');
                }
            }
            return;
        }

        if (row) {
            var fresh = buildRow(s);
            row.replaceWith(fresh);
            flash(fresh);
            AppToast.show('Phiên thi "' + s.title + '" vừa cập nhật lịch', 'info');
        } else if (!isFiltered) {
            var freshRow = buildRow(s);
            tbody.prepend(freshRow);
            flash(freshRow);
            refreshEmptyNote();
        }
    });

    ExamEvents.on('SessionDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một phiên thi vừa được gỡ khỏi lịch', 'warning');
    });

    ExamEvents.on('ExamHubReconnected', function () {
        // Tải lại trang để đồng bộ dữ liệu mới nhất nếu bị mất kết nối lâu
        window.location.reload();
    });
})();
