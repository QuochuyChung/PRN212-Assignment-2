using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class ExamSessionService : IExamSessionService
    {
        private readonly IExamSessionRepository _sessions;
        private readonly ISubjectRepository _subjects;
        private readonly IRoomRepository _rooms;
        private readonly ISlotService _slotService;
        private readonly IRealtimeNotifier _notifier;

        public ExamSessionService(
            IExamSessionRepository sessions,
            ISubjectRepository subjects,
            IRoomRepository rooms,
            ISlotService slotService,
            IRealtimeNotifier notifier)
        {
            _sessions = sessions;
            _subjects = subjects;
            _rooms = rooms;
            _slotService = slotService;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<SessionDto>> GetAllAsync(string? search = null, int? subjectId = null, string? status = null)
        {
            ExamSessionStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ExamSessionStatus>(status, true, out var st))
            {
                parsedStatus = st;
            }

            var list = await _sessions.GetAllAsync(search, subjectId, parsedStatus);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<SessionDto>> GetUpcomingAsync(int? subjectId = null)
        {
            var list = await _sessions.GetUpcomingAsync(subjectId);
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<SessionDto?> GetByIdAsync(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            return session?.ToDto();
        }

        public async Task<ServiceResult<SessionDto>> CreateAsync(SessionSaveDto dto)
        {
            var errors = await ValidateSessionAsync(dto);
            if (errors.Count > 0)
            {
                return ServiceResult<SessionDto>.Failure(errors);
            }

            if (!Enum.TryParse<ExamSessionStatus>(dto.Status, true, out var status))
            {
                status = ExamSessionStatus.Scheduled;
            }

            var startTime = dto.StartTime.Kind == DateTimeKind.Utc
                ? dto.StartTime
                : DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Local).ToUniversalTime();

            var endTime = dto.EndTime.Kind == DateTimeKind.Utc
                ? dto.EndTime
                : DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Local).ToUniversalTime();

            var session = new ExamSession
            {
                Title = dto.Title.Trim(),
                SubjectId = dto.SubjectId,
                RoomId = dto.RoomId,
                StartTime = startTime,
                EndTime = endTime,
                SlotMinutes = dto.SlotMinutes,
                MainQuestionCount = dto.MainQuestionCount,
                MaxFollowUpCount = dto.MaxFollowUpCount,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            await _sessions.AddAsync(session);

            var created = await _sessions.GetByIdAsync(session.Id) ?? session;
            var resultDto = created.ToDto();

            await _notifier.BroadcastAsync(HubEvents.SessionCreated, resultDto);

            return ServiceResult<SessionDto>.Success(resultDto);
        }

        public async Task<ServiceResult<SessionDto>> UpdateAsync(int id, SessionSaveDto dto)
        {
            var session = await _sessions.GetByIdAsync(id);
            if (session == null)
            {
                return ServiceResult<SessionDto>.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            var errors = await ValidateSessionAsync(dto, excludeId: id);
            if (errors.Count > 0)
            {
                return ServiceResult<SessionDto>.Failure(errors);
            }

            if (!Enum.TryParse<ExamSessionStatus>(dto.Status, true, out var status))
            {
                status = session.Status;
            }

            var startTime = dto.StartTime.Kind == DateTimeKind.Utc
                ? dto.StartTime
                : DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Local).ToUniversalTime();

            var endTime = dto.EndTime.Kind == DateTimeKind.Utc
                ? dto.EndTime
                : DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Local).ToUniversalTime();

            var timeChanged = session.StartTime != startTime ||
                              session.EndTime != endTime ||
                              session.SlotMinutes != dto.SlotMinutes;

            session.Title = dto.Title.Trim();
            session.SubjectId = dto.SubjectId;
            session.RoomId = dto.RoomId;
            session.StartTime = startTime;
            session.EndTime = endTime;
            session.SlotMinutes = dto.SlotMinutes;
            session.MainQuestionCount = dto.MainQuestionCount;
            session.MaxFollowUpCount = dto.MaxFollowUpCount;
            session.Status = status;

            if (timeChanged)
            {
                var slotResult = await _slotService.RecalculateSlotsAsync(id);
                if (!slotResult.IsSuccess)
                {
                    return ServiceResult<SessionDto>.Failure(
                        slotResult.Errors.ToDictionary(pair => pair.Key, pair => pair.Value));
                }
            }

            await _sessions.UpdateAsync(session);

            var updated = await _sessions.GetByIdAsync(id) ?? session;
            var resultDto = updated.ToDto();

            await _notifier.BroadcastAsync(HubEvents.SessionUpdated, resultDto);

            return ServiceResult<SessionDto>.Success(resultDto);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            if (session == null)
            {
                return ServiceResult.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            await _slotService.RemoveAllOfSessionAsync(id);

            var deleted = await _sessions.TryDeleteAsync(session);
            if (!deleted)
            {
                return ServiceResult.Failure(string.Empty, "Không thể xóa phiên thi này do có dữ liệu liên quan.");
            }

            await _notifier.BroadcastAsync(HubEvents.SessionDeleted, id);

            return ServiceResult.Success();
        }

        public async Task<ServiceResult<SessionDto>> CancelAsync(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            if (session == null)
            {
                return ServiceResult<SessionDto>.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            session.Status = ExamSessionStatus.Cancelled;
            await _sessions.UpdateAsync(session);

            var updated = await _sessions.GetByIdAsync(id) ?? session;
            var resultDto = updated.ToDto();

            await _notifier.BroadcastAsync(HubEvents.SessionUpdated, resultDto);

            return ServiceResult<SessionDto>.Success(resultDto);
        }

        private async Task<Dictionary<string, string[]>> ValidateSessionAsync(SessionSaveDto dto, int? excludeId = null)
        {
            var errors = new Dictionary<string, string[]>();

            var title = (dto.Title ?? string.Empty).Trim();
            if (title.Length == 0)
            {
                errors[nameof(dto.Title)] = new[] { "Tiêu đề phiên thi không được để trống." };
            }
            else if (title.Length > 150)
            {
                errors[nameof(dto.Title)] = new[] { "Tiêu đề tối đa 150 ký tự." };
            }

            if (dto.EndTime <= dto.StartTime)
            {
                errors[nameof(dto.EndTime)] = new[] { "Thời gian kết thúc phải sau thời gian bắt đầu." };
            }

            if (dto.SlotMinutes < 5 || dto.SlotMinutes > 180)
            {
                errors[nameof(dto.SlotMinutes)] = new[] { "Thời lượng mỗi slot phải từ 5 đến 180 phút." };
            }
            else if (dto.EndTime > dto.StartTime)
            {
                var durationMinutes = (dto.EndTime - dto.StartTime).TotalMinutes;
                if (durationMinutes < dto.SlotMinutes)
                {
                    errors[nameof(dto.SlotMinutes)] = new[] { "Khung giờ phiên thi phải đủ chứa ít nhất 1 slot." };
                }
            }

            if (dto.MainQuestionCount < 1)
            {
                errors[nameof(dto.MainQuestionCount)] = new[] { "Số câu hỏi chính phải từ 1 trở lên." };
            }

            if (dto.MaxFollowUpCount < 0)
            {
                errors[nameof(dto.MaxFollowUpCount)] = new[] { "Số câu hỏi phụ không được âm." };
            }

            var subject = await _subjects.GetByIdAsync(dto.SubjectId);
            if (subject == null)
            {
                errors[nameof(dto.SubjectId)] = new[] { "Môn học không tồn tại." };
            }

            var room = await _rooms.GetByIdAsync(dto.RoomId);
            if (room == null)
            {
                errors[nameof(dto.RoomId)] = new[] { "Phòng thi không tồn tại." };
            }
            else if (!room.IsActive)
            {
                errors[nameof(dto.RoomId)] = new[] { "Phòng thi đã bị khóa, không thể xếp lịch." };
            }

            if (!errors.ContainsKey(nameof(dto.RoomId)) && dto.EndTime > dto.StartTime)
            {
                var conflict = await _sessions.HasRoomConflictAsync(dto.RoomId, dto.StartTime, dto.EndTime, excludeId);
                if (conflict)
                {
                    errors[nameof(dto.RoomId)] = new[] { "Phòng thi này đã có phiên thi khác trùng khung giờ." };
                }
            }

            return errors;
        }
    }
}
