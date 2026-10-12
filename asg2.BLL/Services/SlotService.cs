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

        public SlotService(
            IExamSlotRepository slots,
            IExamSessionRepository sessions,
            IAccountRepository accounts,
            IRealtimeNotifier notifier)
        {
            _slots = slots;
            _sessions = sessions;
            _accounts = accounts;
            _notifier = notifier;
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

            var slots = await _slots.GetBySessionIdAsync(sessionId);
            if (!slots.Any())
            {
                return ServiceResult.Success();
            }

            var updated = new List<ExamSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                var order = i + 1;
                slot.OrderNo = order;
                slot.StartTime = session.StartTime.AddMinutes((order - 1) * session.SlotMinutes);
                slot.EndTime = slot.StartTime.AddMinutes(session.SlotMinutes);
                updated.Add(slot);
            }

            await _slots.UpdateRangeAsync(updated);

            foreach (var slot in updated)
            {
                var dto = slot.ToDto();
                await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotAssigned, dto);
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
                    await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotRemoved, new { slotId = slot.Id, sessionId });
                }
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<SlotDto>> AssignStudentAsync(int sessionId, int studentId)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                return ServiceResult<SlotDto>.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            var student = await _accounts.GetByIdAsync(studentId);
            if (student == null || !student.IsActive)
            {
                return ServiceResult<SlotDto>.Failure("StudentId", "Sinh viên không tồn tại hoặc tài khoản đã bị khóa.");
            }

            var existingSlots = await _slots.GetBySessionIdAsync(sessionId);
            if (existingSlots.Any(s => s.StudentId == studentId))
            {
                return ServiceResult<SlotDto>.Failure("StudentId", "Sinh viên đã được xếp lịch trong phiên thi này.");
            }

            var durationMinutes = (session.EndTime - session.StartTime).TotalMinutes;
            var capacity = session.SlotMinutes > 0 ? (int)Math.Floor(durationMinutes / session.SlotMinutes) : 0;
            if (existingSlots.Count >= capacity)
            {
                return ServiceResult<SlotDto>.Failure("Capacity", $"Phiên thi đã đạt sức chứa tối đa ({capacity} thí sinh).");
            }

            var nextOrder = existingSlots.Count + 1;
            var startTime = session.StartTime.AddMinutes((nextOrder - 1) * session.SlotMinutes);
            var endTime = startTime.AddMinutes(session.SlotMinutes);

            var conflict = await _slots.HasStudentConflictAsync(studentId, startTime, endTime);
            if (conflict)
            {
                return ServiceResult<SlotDto>.Failure("Conflict", "Sinh viên đã có lịch thi khác trùng khung giờ này.");
            }

            var slot = new ExamSlot
            {
                SessionId = sessionId,
                StudentId = studentId,
                OrderNo = nextOrder,
                StartTime = startTime,
                EndTime = endTime
            };

            await _slots.AddAsync(slot);

            var fullSlot = await _slots.GetByIdAsync(slot.Id) ?? slot;
            var dto = fullSlot.ToDto();

            await _notifier.SendToUserAsync(studentId, HubEvents.SlotAssigned, dto);
            await _notifier.SendToAdminsAsync(HubEvents.SlotAssigned, dto);

            return ServiceResult<SlotDto>.Success(dto);
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

            await RecalculateSlotsAsync(sessionId);

            return ServiceResult.Success();
        }
    }
}
