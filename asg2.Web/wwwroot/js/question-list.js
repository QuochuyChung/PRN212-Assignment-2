// =====================================================================================
// question-list.js — Quản lý danh sách câu hỏi realtime qua ExamEvents (P5).
// Sử dụng textContent chống XSS, hiệu ứng row-flash và AppToast khi có thay đổi.
// =====================================================================================
(function () {
    var tbody = document.getElementById('question-list-body');
    var emptyNote = document.getElementById('questions-empty');
    if (!tbody || !window.ExamEvents) { return; }

    function findRow(id) {
        return tbody.querySelector('tr[data-question-id="' + id + '"]');
    }

    function getBloomBadgeClass(level) {
        switch (level) {
            case 1: return 'badge-soft-info';
            case 2: return 'badge-soft-primary';
            case 3: return 'badge-soft-warning';
            case 4: return 'badge-soft-danger';
            default: return 'badge-soft-secondary';
        }
    }

    function getBloomName(level) {
        switch (level) {
            case 1: return 'Nhận biết';
            case 2: return 'Thông hiểu';
            case 3: return 'Vận dụng';
            case 4: return 'Vận dụng cao';
            default: return 'Chưa xác định';
        }
    }

    function buildRow(q) {
        var tr = document.createElement('tr');
        tr.dataset.questionId = q.id;
        tr.dataset.subjectId = q.subjectId;
        tr.dataset.bloom = q.bloomLevel;

        // ID
        var tdId = document.createElement('td');
        tdId.className = 'ps-3 fw-semibold text-muted';
        tdId.textContent = '#' + q.id;
        tr.appendChild(tdId);

        // Subject
        var tdSub = document.createElement('td');
        var subBadge = document.createElement('span');
        subBadge.className = 'badge badge-soft-primary fw-bold';
        subBadge.textContent = q.subjectCode || '';
        tdSub.appendChild(subBadge);

        var subName = document.createElement('div');
        subName.className = 'small text-muted text-truncate';
        subName.style.maxWidth = '140px';
        subName.title = q.subjectName || '';
        subName.textContent = q.subjectName || '';
        tdSub.appendChild(subName);
        tr.appendChild(tdSub);

        // Bloom
        var tdBloom = document.createElement('td');
        var bloomBadge = document.createElement('span');
        bloomBadge.className = 'badge ' + getBloomBadgeClass(q.bloomLevel);
        bloomBadge.textContent = 'Mức ' + q.bloomLevel + ': ' + (q.bloomLevelName || getBloomName(q.bloomLevel));
        tdBloom.appendChild(bloomBadge);
        tr.appendChild(tdBloom);

        // Content
        var tdContent = document.createElement('td');
        var contentDiv = document.createElement('div');
        contentDiv.className = 'question-content fw-medium text-dark text-break';
        contentDiv.textContent = q.content || '';
        tdContent.appendChild(contentDiv);
        tr.appendChild(tdContent);

        // Status
        var tdStatus = document.createElement('td');
        var statusBadge = document.createElement('span');
        statusBadge.className = q.isActive ? 'badge badge-soft-success' : 'badge badge-soft-secondary';
        statusBadge.textContent = q.isActive ? 'Hoạt động' : 'Đã khóa';
        tdStatus.appendChild(statusBadge);
        tr.appendChild(tdStatus);

        // Actions
        var tdActions = document.createElement('td');
        tdActions.className = 'text-end pe-3';

        var divGroup = document.createElement('div');
        divGroup.className = 'd-inline-flex gap-1';

        var btnEdit = document.createElement('a');
        btnEdit.className = 'btn btn-sm btn-outline-primary py-1 px-2';
        btnEdit.href = '/Admin/Questions/Edit/' + encodeURIComponent(q.id);
        btnEdit.textContent = 'Sửa';
        divGroup.appendChild(btnEdit);

        // Toggle form button
        var toggleForm = document.createElement('form');
        toggleForm.method = 'post';
        toggleForm.action = '/Admin/Questions?handler=ToggleActive&id=' + encodeURIComponent(q.id);
        toggleForm.className = 'd-inline m-0';

        var btnToggle = document.createElement('button');
        btnToggle.type = 'submit';
        btnToggle.className = 'btn btn-sm py-1 px-2 ' + (q.isActive ? 'btn-outline-warning' : 'btn-outline-success');
        btnToggle.textContent = q.isActive ? 'Khóa' : 'Mở';
        toggleForm.appendChild(btnToggle);
        divGroup.appendChild(toggleForm);

        // Delete form button
        var deleteForm = document.createElement('form');
        deleteForm.method = 'post';
        deleteForm.action = '/Admin/Questions?handler=Delete&id=' + encodeURIComponent(q.id);
        deleteForm.className = 'd-inline m-0';
        deleteForm.onsubmit = function () {
            return confirm('Bạn có chắc chắn muốn xóa câu hỏi #' + q.id + '?');
        };

        var btnDelete = document.createElement('button');
        btnDelete.type = 'submit';
        btnDelete.className = 'btn btn-sm btn-outline-danger py-1 px-2';
        btnDelete.textContent = 'Xóa';
        deleteForm.appendChild(btnDelete);
        divGroup.appendChild(deleteForm);

        tdActions.appendChild(divGroup);
        tr.appendChild(tdActions);

        return tr;
    }

    function flash(row) {
        row.classList.remove('row-flash');
        void row.offsetWidth;
        row.classList.add('row-flash');
    }

    function updateEmptyState() {
        if (emptyNote) {
            emptyNote.hidden = tbody.children.length > 0;
        }
    }

    ExamEvents.on('QuestionCreated', function (q) {
        if (findRow(q.id)) { return; }
        var row = buildRow(q);
        tbody.prepend(row);
        flash(row);
        updateEmptyState();
        if (window.AppToast) {
            AppToast.show('Đã thêm câu hỏi mới #' + q.id + ' (' + (q.subjectCode || '') + ')', 'info');
        }
    });

    ExamEvents.on('QuestionUpdated', function (q) {
        var row = findRow(q.id);
        if (!row) { return; }
        var fresh = buildRow(q);
        row.replaceWith(fresh);
        flash(fresh);
    });

    ExamEvents.on('QuestionDeleted', function (id) {
        var row = findRow(id);
        if (!row) { return; }
        row.remove();
        updateEmptyState();
        if (window.AppToast) {
            AppToast.show('Câu hỏi #' + id + ' vừa bị xóa.', 'warning');
        }
    });
})();
