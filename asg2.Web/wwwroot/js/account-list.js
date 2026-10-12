// Danh sách tài khoản sinh viên cập nhật realtime khi admin khác thêm / sửa / xóa (xem HubEvents).
(function () {
    var tbody = document.getElementById('accounts-body');
    var emptyNote = document.getElementById('accounts-empty');
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
        badge.className = 'badge ' + (isActive ? 'bg-success' : 'bg-secondary');
        badge.textContent = isActive ? 'Hoạt động' : 'Đã khóa';
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
    function buildRow(a) {
        var tr = document.createElement('tr');
        tr.dataset.id = a.id;

        tr.appendChild(cell('col-username', a.username));
        tr.appendChild(cell('col-fullname', a.fullName));

        var status = cell('col-status');
        status.appendChild(statusBadge(a.isActive));
        tr.appendChild(status);

        var actions = cell('text-end');
        actions.appendChild(actionLink('Sửa', '/Admin/Accounts/Edit?id=' + encodeURIComponent(a.id), 'btn-outline-primary'));
        actions.appendChild(actionLink('Xóa', '/Admin/Accounts/Delete?id=' + encodeURIComponent(a.id), 'btn-outline-danger'));
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

    ExamEvents.on('AccountCreated', function (a) {
        // Đang lọc theo từ khóa thì không tự thêm dòng (có thể không khớp bộ lọc), chỉ báo.
        if (isFiltered || findRow(a.id)) { return; }
        var row = buildRow(a);
        tbody.appendChild(row);
        flash(row);
        refreshEmptyNote();
        AppToast.show('Đã thêm tài khoản ' + a.username, 'info');
    });

    ExamEvents.on('AccountUpdated', function (a) {
        var row = findRow(a.id);
        if (!row) { return; }
        var fresh = buildRow(a);
        row.replaceWith(fresh);
        flash(fresh);
    });

    ExamEvents.on('AccountDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một tài khoản vừa bị xóa', 'warning');
    });
})();
