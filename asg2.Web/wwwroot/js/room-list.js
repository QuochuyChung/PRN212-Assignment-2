// Danh sách phòng thi cập nhật realtime khi admin khác thêm / sửa / xóa (xem HubEvents).
(function () {
    var tbody = document.getElementById('rooms-body');
    var emptyNote = document.getElementById('rooms-empty');
    if (!tbody || !window.ExamEvents) { return; }

    var isFiltered = tbody.dataset.filtered === 'true';

    function findRow(id) {
        return tbody.querySelector('tr[data-id="' + id + '"]');
    }

    function cell(className, text) {
        var td = document.createElement('td');
        if (className) { td.className = className; }
        if (text !== undefined) { td.textContent = text; } // textContent: dữ liệu người dùng không bao giờ thành HTML
        return td;
    }

    function statusBadge(isActive) {
        var badge = document.createElement('span');
        badge.className = 'badge ' + (isActive ? 'text-bg-success' : 'text-bg-secondary');
        badge.textContent = isActive ? 'Hoạt động' : 'Ngừng sử dụng';
        return badge;
    }

    function actionLink(label, href, style) {
        var a = document.createElement('a');
        a.className = 'btn btn-sm ' + style + ' me-1';
        a.href = href;
        a.textContent = label;
        return a;
    }

    // Cùng cấu trúc với dòng server render ở Index.cshtml.
    function buildRow(r) {
        var tr = document.createElement('tr');
        tr.dataset.id = r.id;

        tr.appendChild(cell('col-name', r.name));
        tr.appendChild(cell('col-building', r.building || '—'));

        var status = cell('col-status');
        status.appendChild(statusBadge(r.isActive));
        tr.appendChild(status);

        var actions = cell('text-end');
        actions.appendChild(actionLink('Sửa', '/Admin/Rooms/Edit?id=' + encodeURIComponent(r.id), 'btn-outline-primary'));
        actions.appendChild(actionLink('Xóa', '/Admin/Rooms/Delete?id=' + encodeURIComponent(r.id), 'btn-outline-danger'));
        tr.appendChild(actions);

        return tr;
    }

    function flash(row) {
        row.classList.remove('row-flash');
        void row.offsetWidth; // khởi động lại animation
        row.classList.add('row-flash');
    }

    function refreshEmptyNote() {
        if (emptyNote) { emptyNote.hidden = tbody.children.length > 0; }
    }

    ExamEvents.on('RoomCreated', function (r) {
        // Đang lọc theo từ khóa thì không tự thêm dòng (có thể không khớp bộ lọc), chỉ báo.
        if (isFiltered || findRow(r.id)) { return; }
        var row = buildRow(r);
        tbody.appendChild(row);
        flash(row);
        refreshEmptyNote();
        AppToast.show('Đã thêm phòng ' + r.name, 'info');
    });

    ExamEvents.on('RoomUpdated', function (r) {
        var row = findRow(r.id);
        if (!row) { return; }
        var fresh = buildRow(r);
        row.replaceWith(fresh);
        flash(fresh);
    });

    ExamEvents.on('RoomDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một phòng thi vừa bị xóa', 'warning');
    });
})();
