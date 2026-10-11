// Danh sách môn học cập nhật realtime khi admin khác thêm / sửa / xóa (xem HubEvents).
(function () {
    var tbody = document.getElementById('subjects-body');
    var emptyNote = document.getElementById('subjects-empty');
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

    function actionLink(label, href, style) {
        var a = document.createElement('a');
        a.className = 'btn btn-sm ' + style + ' me-1';
        a.href = href;
        a.textContent = label;
        return a;
    }

    // Cùng cấu trúc với dòng server render ở Index.cshtml.
    function buildRow(s) {
        var tr = document.createElement('tr');
        tr.dataset.id = s.id;

        tr.appendChild(cell('col-code', s.code));
        tr.appendChild(cell('col-name', s.name));

        var actions = cell('text-end');
        actions.appendChild(actionLink('Sửa', '/Admin/Subjects/Edit?id=' + encodeURIComponent(s.id), 'btn-outline-primary'));
        actions.appendChild(actionLink('Xóa', '/Admin/Subjects/Delete?id=' + encodeURIComponent(s.id), 'btn-outline-danger'));
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

    ExamEvents.on('SubjectCreated', function (s) {
        // Đang lọc theo từ khóa thì không tự thêm dòng (có thể không khớp bộ lọc), chỉ báo.
        if (isFiltered || findRow(s.id)) { return; }
        var row = buildRow(s);
        tbody.appendChild(row);
        flash(row);
        refreshEmptyNote();
        AppToast.show('Đã thêm môn học ' + s.name, 'info');
    });

    ExamEvents.on('SubjectUpdated', function (s) {
        var row = findRow(s.id);
        if (!row) { return; }
        var fresh = buildRow(s);
        row.replaceWith(fresh);
        flash(fresh);
    });

    ExamEvents.on('SubjectDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        refreshEmptyNote();
        AppToast.show('Một môn học vừa bị xóa', 'warning');
    });
})();
