using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class SlotService : ISlotService
    {
        private readonly IExamSlotRepository _slots;
        private readonly IExamSessionRepository _sessions;
        private readonly IAccountRepository _accounts;
        private readonly IRealtimeNotifier _notifier;
        private readonly IQuestionPickerService _picker;

        public SlotService(
            IExamSlotRepository slots,
            IExamSessionRepository sessions,
            IAccountRepository accounts,
            IRealtimeNotifier notifier,
            IQuestionPickerService picker)
        {
            _slots = slots;
            _sessions = sessions;
            _accounts = accounts;
            _notifier = notifier;
            _picker = picker;
        }

        public async Task<SlotDto?> GetByIdAsync(int slotId)
        {
            var slot = await _slots.GetByIdAsync(slotId);
            return slot?.ToDto();
        }

        public async Task<IReadOnlyList<SlotDto>> GetSlotsOfSessionAsync(int sessionId)
        {
            var list = await _slots.GetBySessionIdAsync(sessionId);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<SlotDto>> GetMySlotsAsync(int studentId)
        {
            var list = await _slots.GetByStudentIdAsync(studentId);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<ServiceResult> RecalculateSlotsAsync(int sessionId)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                return ServiceResult.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            var slots = session.Slots
                .OrderBy(slot => slot.OrderNo)
                .ThenBy(slot => slot.Id)
                .ToList();
            if (!slots.Any())
            {
                return ServiceResult.Success();
            }

            var durationMinutes = (session.EndTime - session.StartTime).TotalMinutes;
            var capacity = session.SlotMinutes > 0
                ? (int)Math.Floor(durationMinutes / session.SlotMinutes)
                : 0;
            if (slots.Count > capacity)
            {
                return ServiceResult.Failure(
                    "EndTime",
                    $"Khung giờ mới chỉ chứa được {capacity} thí sinh, trong khi phiên đã có {slots.Count} thí sinh.");
            }

            var externalConflicts = await _slots.GetStudentConflictsAsync(
                slots.Select(slot => slot.StudentId),
                session.StartTime,
                session.EndTime,
                sessionId);

            var proposed = new List<(ExamSlot Slot, int OrderNo, DateTime StartTime, DateTime EndTime)>();
            for (var i = 0; i < slots.Count; i++)
            {
                var orderNo = i + 1;
                var startTime = session.StartTime.AddMinutes(i * session.SlotMinutes);
                var endTime = startTime.AddMinutes(session.SlotMinutes);
                var conflict = externalConflicts.Any(other =>
                    other.StudentId == slots[i].StudentId &&
                    other.StartTime < endTime &&
                    startTime < other.EndTime);

                if (conflict)
                {
                    var studentName = slots[i].Student?.FullName ?? slots[i].Student?.Username ?? $"ID {slots[i].StudentId}";
                    return ServiceResult.Failure(
                        "StartTime",
                        $"Không thể đổi giờ vì sinh viên {studentName} đã có ca thi khác trùng khung giờ dự kiến.");
                }

                proposed.Add((slots[i], orderNo, startTime, endTime));
            }

            var updated = new List<ExamSlot>();
            foreach (var item in proposed)
            {
                item.Slot.OrderNo = item.OrderNo;
                item.Slot.StartTime = item.StartTime;
                item.Slot.EndTime = item.EndTime;
                updated.Add(item.Slot);
            }

            await _slots.UpdateRangeAsync(updated);

            foreach (var slot in updated)
            {
                slot.Session = session;
                var dto = slot.ToDto();
                await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotAssigned, dto);
                await _notifier.SendToAdminsAsync(HubEvents.SlotAssigned, dto);
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RemoveAllOfSessionAsync(int sessionId)
        {
            var slots = await _slots.GetBySessionIdAsync(sessionId);
            if (slots.Any())
            {
                await _slots.RemoveRangeAsync(slots);
                foreach (var slot in slots)
                {
                    var payload = new { slotId = slot.Id, sessionId };
                    await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotRemoved, payload);
                    await _notifier.SendToAdminsAsync(HubEvents.SlotRemoved, payload);
                }
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<SlotDto>> AssignStudentAsync(int sessionId, int studentId)
        {
            var result = await AssignStudentsAsync(sessionId, new[] { studentId });
            if (!result.IsSuccess)
            {
                return ServiceResult<SlotDto>.Failure(result.Errors.ToDictionary(pair => pair.Key, pair => pair.Value));
            }

            return ServiceResult<SlotDto>.Success(result.Value![0]);
        }

        public async Task<ServiceResult<IReadOnlyList<SlotDto>>> AssignStudentsAsync(
            int sessionId,
            IEnumerable<int> studentIds)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            if (session.Status != ExamSessionStatus.Scheduled)
            {
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure(
                    string.Empty,
                    "Chỉ có thể gán sinh viên vào phiên thi đang ở trạng thái Sắp tới.");
            }

            var requestedIds = studentIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            if (requestedIds.Count == 0)
            {
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure(
                    "StudentIds",
                    "Vui lòng chọn ít nhất một sinh viên.");
            }

            var existingSlots = await _slots.GetBySessionIdAsync(sessionId);
            var alreadyAssignedIds = existingSlots
                .Where(slot => requestedIds.Contains(slot.StudentId))
                .Select(slot => slot.StudentId)
                .ToHashSet();
            if (alreadyAssignedIds.Count > 0)
            {
                var names = existingSlots
                    .Where(slot => alreadyAssignedIds.Contains(slot.StudentId))
                    .Select(slot => slot.Student?.FullName ?? slot.Student?.Username ?? $"ID {slot.StudentId}");
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure(
                    "StudentIds",
                    $"Các sinh viên sau đã được gán vào phiên này: {string.Join(", ", names)}.");
            }

            var durationMinutes = (session.EndTime - session.StartTime).TotalMinutes;
            var capacity = session.SlotMinutes > 0 ? (int)Math.Floor(durationMinutes / session.SlotMinutes) : 0;
            var usedOrders = existingSlots.Select(slot => slot.OrderNo).ToHashSet();
            var availableOrders = Enumerable.Range(1, capacity)
                .Where(orderNo => !usedOrders.Contains(orderNo))
                .ToList();
            if (requestedIds.Count > availableOrders.Count)
            {
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure(
                    "Capacity",
                    $"Phiên chỉ còn {availableOrders.Count} chỗ trống (sức chứa tối đa {capacity} thí sinh), không đủ cho {requestedIds.Count} sinh viên đã chọn.");
            }

            var errors = new List<string>();
            var candidates = new List<(Account Student, List<int> Orders)>();
            var externalConflicts = await _slots.GetStudentConflictsAsync(
                requestedIds,
                session.StartTime,
                session.EndTime);

            foreach (var studentId in requestedIds)
            {
                var student = await _accounts.GetByIdAsync(studentId);
                if (student == null || !student.IsActive || student.Role != AccountRole.Student)
                {
                    errors.Add($"Sinh viên ID {studentId} không tồn tại hoặc tài khoản đã bị khóa.");
                    continue;
                }

                var compatibleOrders = new List<int>();
                foreach (var orderNo in availableOrders)
                {
                    var startTime = session.StartTime.AddMinutes((orderNo - 1) * session.SlotMinutes);
                    var endTime = startTime.AddMinutes(session.SlotMinutes);
                    var hasConflict = externalConflicts.Any(other =>
                        other.StudentId == studentId &&
                        other.StartTime < endTime &&
                        startTime < other.EndTime);
                    if (!hasConflict)
                    {
                        compatibleOrders.Add(orderNo);
                    }
                }

                if (compatibleOrders.Count == 0)
                {
                    errors.Add($"Sinh viên {student.FullName} đã có lịch thi trùng với tất cả khung giờ còn trống.");
                    continue;
                }

                candidates.Add((student, compatibleOrders));
            }

            if (errors.Count > 0)
            {
                return ServiceResult<IReadOnlyList<SlotDto>>.Failure("StudentIds", errors.ToArray());
            }

            // Ghép sinh viên với slot theo kiểu bipartite matching. Cách này tránh trường hợp
            // sinh viên linh hoạt chiếm mất slot duy nhất mà sinh viên khác có thể dự thi.
            var studentByOrder = new Dictionary<int, int>();
            var candidateByStudentId = candidates.ToDictionary(candidate => candidate.Student.Id);

            bool TryMatch(int studentId, HashSet<int> visitedOrders)
            {
                foreach (var orderNo in candidateByStudentId[studentId].Orders.OrderBy(order => order))
                {
                    if (!visitedOrders.Add(orderNo))
                    {
                        continue;
                    }

                    if (!studentByOrder.TryGetValue(orderNo, out var currentStudentId)
                        || TryMatch(currentStudentId, visitedOrders))
                    {
                        studentByOrder[orderNo] = studentId;
                        return true;
                    }
                }

                return false;
            }

            foreach (var candidate in candidates.OrderBy(candidate => candidate.Orders.Count))
            {
                if (!TryMatch(candidate.Student.Id, new HashSet<int>()))
                {
                    return ServiceResult<IReadOnlyList<SlotDto>>.Failure(
                        "StudentIds",
                        "Không thể xếp đồng thời tất cả sinh viên đã chọn mà không trùng lịch. Hãy chọn ít sinh viên hơn hoặc điều chỉnh phiên thi.");
                }
            }

            var orderByStudent = studentByOrder.ToDictionary(pair => pair.Value, pair => pair.Key);
            var newSlots = candidates.Select(candidate =>
            {
                var orderNo = orderByStudent[candidate.Student.Id];
                var slotStart = session.StartTime.AddMinutes((orderNo - 1) * session.SlotMinutes);
                return new ExamSlot
                {
                    SessionId = sessionId,
                    Session = session,
                    StudentId = candidate.Student.Id,
                    Student = candidate.Student,
                    OrderNo = orderNo,
                    StartTime = slotStart,
                    EndTime = slotStart.AddMinutes(session.SlotMinutes)
                };
            }).ToList();

            await _slots.AddRangeAsync(newSlots);

            foreach (var slot in newSlots.OrderBy(s => s.OrderNo))
            {
                await _picker.PickForSlotAsync(slot.Id);
            }

            var resultDtos = newSlots
                .OrderBy(slot => slot.OrderNo)
                .Select(slot => slot.ToDto())
                .ToList();

            foreach (var dto in resultDtos)
            {
                await _notifier.SendToUserAsync(dto.StudentId, HubEvents.SlotAssigned, dto);
                await _notifier.SendToAdminsAsync(HubEvents.SlotAssigned, dto);
            }

            return ServiceResult<IReadOnlyList<SlotDto>>.Success(resultDtos);
        }

        public async Task<ServiceResult> RemoveStudentAsync(int slotId)
        {
            var slot = await _slots.GetByIdAsync(slotId);
            if (slot == null)
            {
                return ServiceResult.Failure(string.Empty, "Không tìm thấy ca thi.");
            }

            var sessionId = slot.SessionId;
            var studentId = slot.StudentId;

            await _slots.RemoveRangeAsync(new[] { slot });

            await _notifier.SendToUserAsync(studentId, HubEvents.SlotRemoved, new { slotId, sessionId });
            await _notifier.SendToAdminsAsync(HubEvents.SlotRemoved, new { slotId, sessionId });

            return ServiceResult.Success();
        }
    }
}
