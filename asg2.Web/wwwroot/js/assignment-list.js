// Trang gán sinh viên: lọc danh sách và cập nhật slot trực tiếp qua bus SignalR dùng chung.
(function () {
    var page = document.getElementById('assignment-page');
    if (!page || !window.ExamEvents) { return; }

    var sessionId = Number(page.dataset.sessionId);
    var tbody = document.getElementById('assignment-slots-body');
    var emptyNote = document.getElementById('assignment-slots-empty');
    var studentList = document.getElementById('student-list');
    var studentSearch = document.getElementById('student-search');
    var studentEmpty = document.getElementById('student-empty');
    var assignButton = document.getElementById('assign-button');
    var assignForm = document.getElementById('assign-form');
    var countElement = document.getElementById('assignment-count');
    var capacityElement = document.getElementById('assignment-capacity');
    var remainingElement = document.getElementById('assignment-remaining');
    var selectedCountElement = document.getElementById('selected-student-count');
    var assignButtonCount = document.getElementById('assign-button-count');
    var selectVisibleButton = document.getElementById('select-visible-students');
    var clearSelectionButton = document.getElementById('clear-student-selection');
    var assignmentEnabled = true;
    var questionsBySlot = Object.create(null);

    function formatDateTime(value) {
        var date = new Date(value);
        var pad = function (number) { return number < 10 ? '0' + number : String(number); };
        return pad(date.getDate()) + '/' + pad(date.getMonth() + 1) + '/' + date.getFullYear()
            + ' ' + pad(date.getHours()) + ':' + pad(date.getMinutes());
    }

    function formatHour(value) {
        var date = new Date(value);
        var pad = function (number) { return number < 10 ? '0' + number : String(number); };
        return pad(date.getHours()) + ':' + pad(date.getMinutes());
    }

    function findSlotRow(slotId) {
        return tbody.querySelector('tr[data-slot-id="' + slotId + '"]');
    }

    function findStudentChoice(studentId) {
        return studentList.querySelector('[data-student-id="' + studentId + '"]');
    }

    function setStudentAssigned(studentId, assigned) {
        var choice = findStudentChoice(studentId);
        if (!choice) { return; }

        var checkbox = choice.querySelector('input[type="checkbox"]');
        var badge = choice.querySelector('.assigned-label');
        checkbox.checked = false;
        checkbox.disabled = assigned;
        choice.dataset.assigned = assigned ? 'true' : 'false';
        choice.classList.toggle('bg-light', assigned);
        choice.classList.toggle('text-muted', assigned);
        if (badge) { badge.hidden = !assigned; }
    }

    function createCell(className) {
        var cell = document.createElement('td');
        cell.className = className;
        return cell;
    }

    function appendHidden(form, name, value) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        form.appendChild(input);
    }

    function buildPostForm(handler, slot, buttonText, buttonClass, title) {
        var form = document.createElement('form');
        form.method = 'post';
        form.action = window.location.pathname + '?handler=' + handler;
        form.className = 'd-inline m-0' + (handler === 'Remove' ? ' remove-slot-form' : '');

        var token = assignForm.querySelector('input[name="__RequestVerificationToken"]');
        if (token) { appendHidden(form, token.name, token.value); }
        appendHidden(form, 'sessionId', sessionId);
        appendHidden(form, 'slotId', slot.id);

        var button = document.createElement('button');
        button.type = 'submit';
        button.className = buttonClass;
        button.textContent = buttonText;
        if (title) { button.title = title; }
        form.appendChild(button);
        return form;
    }

    function buildQuestionsCell(slotId) {
        var questionsCell = createCell('col-questions');
        var wrapper = document.createElement('div');
        wrapper.className = 'd-flex flex-wrap gap-1 align-items-center';
        var questions = questionsBySlot[slotId] || [];

        if (questions.length === 0) {
            var empty = document.createElement('span');
            empty.className = 'small text-muted';
            empty.textContent = 'Chưa bốc';
            wrapper.appendChild(empty);
        } else {
            questions.forEach(function (question) {
                var badge = document.createElement('span');
                badge.className = 'badge badge-soft-info';
                badge.title = question.content;
                badge.style.cursor = 'help';
                badge.textContent = '#' + question.orderNo + ' (Mức ' + question.bloomLevel + ')';
                wrapper.appendChild(badge);
            });
        }

        questionsCell.appendChild(wrapper);
        return questionsCell;
    }

    function buildSlotRow(slot) {
        var row = document.createElement('tr');
        row.dataset.slotId = slot.id;
        row.dataset.studentId = slot.studentId;
        row.dataset.orderNo = slot.orderNo;

        var orderCell = createCell('col-order ps-3');
        var orderBadge = document.createElement('span');
        orderBadge.className = 'badge badge-soft-primary';
        orderBadge.textContent = '#' + slot.orderNo;
        orderCell.appendChild(orderBadge);
        row.appendChild(orderCell);

        var studentCell = createCell('col-student');
        var fullName = document.createElement('div');
        fullName.className = 'fw-semibold student-fullname';
        fullName.textContent = slot.studentFullName;
        studentCell.appendChild(fullName);
        var username = document.createElement('div');
        username.className = 'small text-muted student-username';
        username.textContent = slot.studentUsername;
        studentCell.appendChild(username);
        row.appendChild(studentCell);

        var timeCell = createCell('col-time');
        var timeBadge = document.createElement('span');
        timeBadge.className = 'badge badge-soft-success';
        timeBadge.textContent = formatHour(slot.startTime) + ' – ' + formatHour(slot.endTime);
        timeCell.appendChild(timeBadge);
        row.appendChild(timeCell);

        row.appendChild(buildQuestionsCell(slot.id));

        var actionCell = createCell('text-end pe-3');
        var actionWrapper = document.createElement('div');
        actionWrapper.className = 'd-inline-flex gap-1 align-items-center';
        actionWrapper.appendChild(buildPostForm(
            'RepickSlot',
            slot,
            'Bốc lại',
            'btn btn-sm btn-outline-secondary py-1 px-2',
            'Bốc lại câu hỏi cho riêng slot này'));
        actionWrapper.appendChild(buildPostForm(
            'Remove',
            slot,
            'Gỡ',
            'btn btn-sm btn-outline-danger py-1 px-2'));
        actionCell.appendChild(actionWrapper);
        row.appendChild(actionCell);
        return row;
    }

    function insertInOrder(row) {
        var orderNo = Number(row.dataset.orderNo);
        var rows = Array.from(tbody.querySelectorAll('tr[data-order-no]'));
        var next = rows.find(function (candidate) {
            return Number(candidate.dataset.orderNo) > orderNo;
        });
        tbody.insertBefore(row, next || null);
    }

    function flash(row) {
        row.classList.remove('row-flash');
        void row.offsetWidth;
        row.classList.add('row-flash');
    }

    function getRemainingCapacity() {
        var assignedCount = tbody.querySelectorAll('tr[data-slot-id]').length;
        var capacity = Number(page.dataset.capacity || 0);
        return Math.max(0, capacity - assignedCount);
    }

    function updateSelection() {
        var remaining = getRemainingCapacity();
        var checkboxes = Array.from(studentList.querySelectorAll('input[type="checkbox"]'));
        var selected = checkboxes.filter(function (checkbox) { return checkbox.checked; });
        var limitReached = selected.length >= remaining;

        checkboxes.forEach(function (checkbox) {
            var choice = checkbox.closest('.student-choice');
            var assigned = choice && choice.dataset.assigned === 'true';
            checkbox.disabled = assigned || !assignmentEnabled || (limitReached && !checkbox.checked);
        });

        selectedCountElement.textContent = selected.length;
        assignButtonCount.textContent = selected.length;
        assignButton.disabled = !assignmentEnabled || remaining === 0 || selected.length === 0;
        clearSelectionButton.disabled = selected.length === 0;
        selectVisibleButton.disabled = !assignmentEnabled || remaining === 0;
    }

    function updateCapacity() {
        var assignedCount = tbody.querySelectorAll('tr[data-slot-id]').length;
        var capacity = Number(page.dataset.capacity || 0);
        var remaining = getRemainingCapacity();
        countElement.textContent = assignedCount;
        capacityElement.textContent = capacity;
        remainingElement.textContent = remaining;
        emptyNote.hidden = assignedCount > 0;
        updateSelection();
    }

    function filterStudents() {
        var term = studentSearch.value.trim().toLocaleLowerCase('vi');
        var visibleCount = 0;
        studentList.querySelectorAll('.student-choice').forEach(function (choice) {
            var visible = !term || choice.dataset.search.indexOf(term) >= 0;
            choice.hidden = !visible;
            if (visible) { visibleCount += 1; }
        });
        studentEmpty.hidden = visibleCount > 0;
    }

    studentSearch.addEventListener('input', filterStudents);
    studentList.addEventListener('change', function (event) {
        if (event.target.matches('input[type="checkbox"]')) {
            updateSelection();
        }
    });

    selectVisibleButton.addEventListener('click', function () {
        var remaining = getRemainingCapacity();
        var selectedCount = studentList.querySelectorAll('input[type="checkbox"]:checked').length;
        var selectable = Array.from(studentList.querySelectorAll('.student-choice'))
            .filter(function (choice) {
                var checkbox = choice.querySelector('input[type="checkbox"]');
                return !choice.hidden && choice.dataset.assigned !== 'true' && !checkbox.checked;
            });

        selectable.slice(0, Math.max(0, remaining - selectedCount)).forEach(function (choice) {
            choice.querySelector('input[type="checkbox"]').checked = true;
        });
        updateSelection();
    });

    clearSelectionButton.addEventListener('click', function () {
        studentList.querySelectorAll('input[type="checkbox"]:checked').forEach(function (checkbox) {
            checkbox.checked = false;
        });
        updateSelection();
    });

    ExamEvents.on('SlotAssigned', function (slot) {
        if (Number(slot.sessionId) !== sessionId) { return; }

        var existing = findSlotRow(slot.id);
        var row = buildSlotRow(slot);
        if (existing) {
            existing.replaceWith(row);
        } else {
            insertInOrder(row);
        }
        setStudentAssigned(slot.studentId, true);
        updateCapacity();
        flash(row);
        AppToast.show('Đã cập nhật slot #' + slot.orderNo + ' của ' + slot.studentFullName + '.', 'success');
    });

    ExamEvents.on('SlotRemoved', function (payload) {
        var removedSessionId = typeof payload === 'object' && payload !== null
            ? Number(payload.sessionId)
            : sessionId;
        if (removedSessionId !== sessionId) { return; }

        var slotId = typeof payload === 'object' && payload !== null ? payload.slotId : payload;
        var row = findSlotRow(slotId);
        if (!row) { return; }
        var studentId = Number(row.dataset.studentId);
        row.remove();
        setStudentAssigned(studentId, false);
        updateCapacity();
        AppToast.show('Một sinh viên vừa được gỡ khỏi phiên thi.', 'warning');
    });

    ExamEvents.on('SlotQuestionsUpdated', function (payload) {
        if (!payload || Number(payload.sessionId) !== sessionId) { return; }

        questionsBySlot[payload.slotId] = payload.questions || [];
        var row = findSlotRow(payload.slotId);
        if (!row) { return; }

        var oldCell = row.querySelector('.col-questions');
        var newCell = buildQuestionsCell(payload.slotId);
        if (oldCell) { oldCell.replaceWith(newCell); }
        flash(row);
    });

    ExamEvents.on('SessionUpdated', function (updatedSession) {
        if (Number(updatedSession.id) !== sessionId) { return; }

        page.dataset.capacity = updatedSession.capacity;
        document.getElementById('assignment-subject').textContent = updatedSession.subjectCode + ' — ' + updatedSession.subjectName;
        document.getElementById('assignment-room').textContent = updatedSession.roomName;
        document.getElementById('assignment-time').textContent = formatDateTime(updatedSession.startTime)
            + ' – ' + formatHour(updatedSession.endTime);
        document.getElementById('assignment-slot-minutes').textContent = updatedSession.slotMinutes + ' phút / sinh viên';
        updateCapacity();

        if (updatedSession.status !== 'Scheduled') {
            assignmentEnabled = false;
            updateSelection();
            AppToast.show('Phiên thi không còn ở trạng thái Sắp tới nên không thể gán thêm sinh viên.', 'warning');
        }
    });

    ExamEvents.on('SessionDeleted', function (deletedSessionId) {
        if (Number(deletedSessionId) !== sessionId) { return; }
        assignmentEnabled = false;
        updateSelection();
        AppToast.show('Phiên thi này vừa bị xóa.', 'warning');
    });

    ExamEvents.on('ExamHubReconnected', function () {
        window.location.reload();
    });

    updateCapacity();
})();
