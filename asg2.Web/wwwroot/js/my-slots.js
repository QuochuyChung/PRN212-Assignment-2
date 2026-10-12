// Lịch thi của tôi (cập nhật realtime khi được gán hoặc đổi giờ slot)
(function () {
    var tbody = document.getElementById('my-slots-body');
    var emptyNote = document.getElementById('my-slots-empty');
    if (!tbody || !window.ExamEvents) { return; }

    function findRow(slotId) {
        return tbody.querySelector('tr[data-slot-id="' + slotId + '"]');
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

    function buildRow(slot) {
        var tr = document.createElement('tr');
        tr.dataset.slotId = slot.id;
        tr.dataset.sessionId = slot.sessionId;

        // Session Title
        var tdTitle = cell('col-title ps-3 fw-bold');
        tdTitle.textContent = slot.sessionTitle;
        tr.appendChild(tdTitle);

        // Subject
        var tdSubject = cell('col-subject');
        var badgeSub = document.createElement('span');
        badgeSub.className = 'badge badge-soft-primary';
        badgeSub.textContent = slot.subjectCode;
        tdSubject.appendChild(badgeSub);
        var divSubName = document.createElement('div');
        divSubName.className = 'small text-muted';
        divSubName.textContent = slot.subjectName;
        tdSubject.appendChild(divSubName);
        tr.appendChild(tdSubject);

        // Room
        var tdRoom = cell('col-room');
        var badgeRoom = document.createElement('span');
        badgeRoom.className = 'badge badge-soft-info';
        badgeRoom.textContent = slot.roomName;
        tdRoom.appendChild(badgeRoom);
        tr.appendChild(tdRoom);

        // Date
        var tdDate = cell('col-date');
        tdDate.textContent = formatDateOnly(slot.startTime);
        tr.appendChild(tdDate);

        // Hours
        var tdHours = cell('col-hours');
        var badgeHours = document.createElement('span');
        badgeHours.className = 'badge badge-soft-success fs-6 fw-bold';
        badgeHours.textContent = formatHourOnly(slot.startTime) + ' - ' + formatHourOnly(slot.endTime);
        tdHours.appendChild(badgeHours);
        tr.appendChild(tdHours);

        // Order
        var tdOrder = cell('col-order');
        var badgeOrder = document.createElement('span');
        badgeOrder.className = 'badge badge-soft-primary px-2 py-1 fw-bold';
        badgeOrder.textContent = 'Slot #' + slot.orderNo;
        tdOrder.appendChild(badgeOrder);
        tr.appendChild(tdOrder);

        // Questions
        var tdQues = cell('col-questions pe-3 small text-secondary');
        tdQues.textContent = slot.mainQuestionCount + ' câu chính';
        tr.appendChild(tdQues);

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

    // Khi giáo viên gán sinh viên hoặc đổi giờ phiên -> bắn SlotAssigned
    ExamEvents.on('SlotAssigned', function (slot) {
        var existing = findRow(slot.id);
        if (existing) {
            var updated = buildRow(slot);
            existing.replaceWith(updated);
            flash(updated);
            AppToast.show('Giờ thi của bạn đã được cập nhật: ' + formatHourOnly(slot.startTime) + ' - ' + formatHourOnly(slot.endTime), 'info');
        } else {
            var row = buildRow(slot);
            tbody.appendChild(row);
            flash(row);
            refreshEmptyNote();
            AppToast.show('Bạn đã được xếp lịch thi: ' + slot.subjectCode + ' (' + slot.roomName + ')', 'success');
        }
    });

    // Khi giáo viên gỡ sinh viên khỏi ca
    ExamEvents.on('SlotRemoved', function (data) {
        var slotId = typeof data === 'object' && data !== null ? data.slotId : data;
        var row = findRow(slotId);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một ca thi của bạn vừa được gỡ khỏi lịch', 'warning');
    });

    // Khi phiên thi bị xóa
    ExamEvents.on('SessionDeleted', function (sessionId) {
        var rows = tbody.querySelectorAll('tr[data-session-id="' + sessionId + '"]');
        if (rows.length > 0) {
            rows.forEach(function (r) { r.remove(); });
            refreshEmptyNote();
            AppToast.show('Phiên thi đã bị hủy/xóa, ca thi của bạn đã được gỡ', 'warning');
        }
    });

    ExamEvents.on('ExamHubReconnected', function () {
        window.location.reload();
    });
})();
