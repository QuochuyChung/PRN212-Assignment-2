// Danh sách phiên thi admin cập nhật realtime (HubEvents.SessionCreated/Updated/Deleted)
(function () {
    var tbody = document.getElementById('sessions-body');
    var emptyNote = document.getElementById('sessions-empty');
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

    function formatTime(isoString) {
        var d = new Date(isoString);
        var pad = function (n) { return n < 10 ? '0' + n : n; };
        return pad(d.getDate()) + '/' + pad(d.getMonth() + 1) + '/' + d.getFullYear() + ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes());
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

    function actionLink(label, href, style) {
        var a = document.createElement('a');
        a.className = 'btn btn-sm ' + style + ' me-1';
        a.href = href;
        a.textContent = label;
        return a;
    }

    function buildRow(s) {
        var tr = document.createElement('tr');
        tr.dataset.id = s.id;

        // Title
        var tdTitle = cell('col-title ps-3 fw-bold');
        var aTitle = document.createElement('a');
        aTitle.href = '/Admin/Sessions/Details?id=' + encodeURIComponent(s.id);
        aTitle.className = 'text-decoration-none text-dark hover-primary';
        aTitle.textContent = s.title;
        tdTitle.appendChild(aTitle);
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

        // Time
        var tdTime = cell('col-time small');
        var divDate = document.createElement('div');
        divDate.className = 'fw-semibold';
        divDate.textContent = formatDateOnly(s.startTime);
        tdTime.appendChild(divDate);
        var divHours = document.createElement('div');
        divHours.className = 'text-muted';
        divHours.textContent = formatHourOnly(s.startTime) + ' - ' + formatHourOnly(s.endTime);
        tdTime.appendChild(divHours);
        tr.appendChild(tdTime);

        // Slot / Questions
        var tdSlot = cell('col-slot small');
        var divSlotMin = document.createElement('div');
        divSlotMin.textContent = s.slotMinutes + ' phút/slot';
        tdSlot.appendChild(divSlotMin);
        var divQues = document.createElement('div');
        divQues.className = 'text-muted';
        divQues.textContent = s.mainQuestionCount + ' câu chính (+' + s.maxFollowUpCount + ' phụ)';
        tdSlot.appendChild(divQues);
        tr.appendChild(tdSlot);

        // Status
        var tdStatus = cell('col-status');
        var badgeStatus = document.createElement('span');
        if (s.status === 'Scheduled') {
            badgeStatus.className = 'badge badge-soft-success';
            badgeStatus.textContent = 'Sắp tới';
        } else if (s.status === 'Cancelled') {
            badgeStatus.className = 'badge badge-soft-danger';
            badgeStatus.textContent = 'Đã hủy';
        } else {
            badgeStatus.className = 'badge badge-soft-secondary';
            badgeStatus.textContent = 'Hoàn thành';
        }
        tdStatus.appendChild(badgeStatus);
        tr.appendChild(tdStatus);

        // Capacity
        var tdCap = cell('col-capacity');
        var badgeCap = document.createElement('span');
        badgeCap.className = 'badge ' + (s.assignedCount >= s.capacity ? 'badge-soft-warning' : 'badge-soft-secondary');
        badgeCap.textContent = s.assignedCount + ' / ' + s.capacity + ' thí sinh';
        tdCap.appendChild(badgeCap);
        tr.appendChild(tdCap);

        // Actions
        var tdActions = cell('text-end pe-3');
        tdActions.appendChild(actionLink('Chi tiết', '/Admin/Sessions/Details?id=' + encodeURIComponent(s.id), 'btn-outline-secondary'));
        tdActions.appendChild(actionLink('Sửa', '/Admin/Sessions/Edit?id=' + encodeURIComponent(s.id), 'btn-outline-primary'));
        tdActions.appendChild(actionLink('Xóa', '/Admin/Sessions/Delete?id=' + encodeURIComponent(s.id), 'btn-outline-danger'));
        tr.appendChild(tdActions);

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
        if (isFiltered || findRow(s.id)) { return; }
        var row = buildRow(s);
        tbody.prepend(row);
        flash(row);
        refreshEmptyNote();
        AppToast.show('Đã tạo phiên thi mới: ' + s.title, 'info');
    });

    ExamEvents.on('SessionUpdated', function (s) {
        var row = findRow(s.id);
        if (!row) { return; }
        var fresh = buildRow(s);
        row.replaceWith(fresh);
        flash(fresh);
        if (s.status === 'Cancelled') {
            AppToast.show('Phiên thi "' + s.title + '" vừa bị hủy', 'warning');
        }
    });

    ExamEvents.on('SessionDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một phiên thi vừa bị xóa', 'warning');
    });
})();
