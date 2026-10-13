// =====================================================================================
// slot-detail.js — Cập nhật bộ câu hỏi của slot thi realtime qua SignalR (P5).
// Sử dụng textContent chống XSS, hiệu ứng row-flash và AppToast khi bốc lại đề.
// =====================================================================================
(function () {
    var container = document.getElementById('slot-detail-container');
    var list = document.getElementById('questions-list');
    var emptyNote = document.getElementById('questions-empty');
    var countEl = document.getElementById('slot-question-count');

    if (!container || !list || !window.ExamEvents) { return; }

    var currentSlotId = parseInt(container.dataset.slotId, 10);

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

    function renderQuestionCard(q) {
        var card = document.createElement('div');
        card.className = 'card shadow-sm question-item-card';
        card.dataset.questionId = q.questionId;
        card.dataset.orderNo = q.orderNo;

        // Header
        var header = document.createElement('div');
        header.className = 'card-header bg-white py-3 d-flex justify-content-between align-items-center';

        var titleBox = document.createElement('div');
        titleBox.className = 'fw-bold text-dark d-flex align-items-center gap-2';

        var orderBadge = document.createElement('span');
        orderBadge.className = 'badge badge-soft-primary px-2 py-1';
        orderBadge.textContent = 'Câu hỏi #' + q.orderNo;
        titleBox.appendChild(orderBadge);
        header.appendChild(titleBox);

        var bloomBadge = document.createElement('span');
        bloomBadge.className = 'badge px-3 py-1 ' + getBloomBadgeClass(q.bloomLevel);
        bloomBadge.textContent = 'Mức ' + q.bloomLevel + ': ' + (q.bloomLevelName || getBloomName(q.bloomLevel));
        header.appendChild(bloomBadge);
        card.appendChild(header);

        // Body
        var body = document.createElement('div');
        body.className = 'card-body p-4';

        var pContent = document.createElement('p');
        pContent.className = 'card-text fs-6 mb-0 text-dark lh-base question-item-content';
        pContent.textContent = q.content || '';
        body.appendChild(pContent);
        card.appendChild(body);

        return card;
    }

    function flash(el) {
        el.classList.remove('row-flash');
        void el.offsetWidth;
        el.classList.add('row-flash');
    }

    ExamEvents.on('SlotQuestionsUpdated', function (payload) {
        if (!payload || parseInt(payload.slotId, 10) !== currentSlotId) {
            return;
        }

        var questions = payload.questions || [];

        // Xóa danh sách cũ
        while (list.firstChild) {
            list.removeChild(list.firstChild);
        }

        if (questions.length === 0) {
            if (emptyNote) { emptyNote.hidden = false; }
            if (countEl) { countEl.textContent = '0 câu hỏi'; }
        } else {
            if (emptyNote) { emptyNote.hidden = true; }
            if (countEl) { countEl.textContent = questions.length + ' câu hỏi'; }

            questions.forEach(function (q) {
                var card = renderQuestionCard(q);
                list.appendChild(card);
                flash(card);
            });
        }

        if (window.AppToast) {
            AppToast.show('Bộ câu hỏi của bạn vừa được cập nhật mới!', 'info');
        }
    });

    ExamEvents.on('ExamHubReconnected', function () {
        // Tải lại để đảm bảo dữ liệu mới nhất sau khi mất kết nối
        window.location.reload();
    });
})();
