using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questions;
        private readonly ISubjectRepository _subjects;
        private readonly IRealtimeNotifier _notifier;

        public QuestionService(
            IQuestionRepository questions,
            ISubjectRepository subjects,
            IRealtimeNotifier notifier)
        {
            _questions = questions;
            _subjects = subjects;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<QuestionDto>> GetAllAsync(
            int? subjectId = null,
            int? bloomLevel = null,
            string? search = null,
            bool includeInactive = true)
        {
            var list = await _questions.GetAllAsync(subjectId, bloomLevel, search, includeInactive);
            return list.Select(q => q.ToDto()).ToList();
        }

        public async Task<QuestionPagedResultDto> GetPagedAsync(
            int pageIndex,
            int pageSize,
            int? subjectId = null,
            int? bloomLevel = null,
            string? search = null,
            bool includeInactive = true)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, totalCount) = await _questions.GetPagedAsync(pageIndex, pageSize, subjectId, bloomLevel, search, includeInactive);
            return new QuestionPagedResultDto
            {
                Items = items.Select(q => q.ToDto()).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<QuestionStatsDto> GetStatsAsync(int? subjectId = null)
        {
            var total = await _questions.CountAsync(includeInactive: true);
            var active = await _questions.CountAsync(includeInactive: false);
            var dist = await _questions.GetBloomDistributionAsync(subjectId);

            return new QuestionStatsDto
            {
                TotalQuestions = total,
                ActiveQuestions = active,
                BloomLevel1Count = dist.TryGetValue(1, out var c1) ? c1 : 0,
                BloomLevel2Count = dist.TryGetValue(2, out var c2) ? c2 : 0,
                BloomLevel3Count = dist.TryGetValue(3, out var c3) ? c3 : 0,
                BloomLevel4Count = dist.TryGetValue(4, out var c4) ? c4 : 0
            };
        }

        public async Task<QuestionDto?> GetByIdAsync(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            return question?.ToDto();
        }

        public async Task<ServiceResult<QuestionDto>> CreateAsync(QuestionSaveDto input)
        {
            var errors = new Dictionary<string, string[]>();

            var subject = await _subjects.GetByIdAsync(input.SubjectId);
            if (subject == null)
            {
                errors["SubjectId"] = new[] { "Môn học đã chọn không tồn tại." };
            }

            var trimmedContent = input.Content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedContent))
            {
                errors["Content"] = new[] { "Nội dung câu hỏi không được để trống." };
            }
            else if (trimmedContent.Length > 2000)
            {
                errors["Content"] = new[] { "Nội dung câu hỏi không được vượt quá 2000 ký tự." };
            }
            else if (subject != null && await _questions.ExistsContentInSubjectAsync(input.SubjectId, trimmedContent))
            {
                errors["Content"] = new[] { "Câu hỏi với nội dung này đã tồn tại trong môn học đã chọn." };
            }

            if (input.BloomLevel < 1 || input.BloomLevel > 4)
            {
                errors["BloomLevel"] = new[] { "Mức độ Bloom phải nằm trong khoảng từ 1 (Nhận biết) đến 4 (Vận dụng cao)." };
            }

            if (errors.Count > 0)
            {
                return ServiceResult<QuestionDto>.Failure(errors);
            }

            var question = new Question
            {
                SubjectId = input.SubjectId,
                Content = trimmedContent,
                BloomLevel = input.BloomLevel,
                IsActive = input.IsActive
            };

            await _questions.AddAsync(question);

            // Tải lại kèm Subject để đầy đủ thông tin mã/tên môn
            var created = await _questions.GetByIdAsync(question.Id) ?? question;
            var dto = created.ToDto();

            await _notifier.SendToAdminsAsync(HubEvents.QuestionCreated, dto);
            return ServiceResult<QuestionDto>.Success(dto);
        }

        public async Task<ServiceResult<QuestionDto>> UpdateAsync(int id, QuestionSaveDto input)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null)
            {
                return ServiceResult<QuestionDto>.Failure(string.Empty, "Không tìm thấy câu hỏi.");
            }

            var errors = new Dictionary<string, string[]>();

            var subject = await _subjects.GetByIdAsync(input.SubjectId);
            if (subject == null)
            {
                errors["SubjectId"] = new[] { "Môn học đã chọn không tồn tại." };
            }

            var trimmedContent = input.Content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedContent))
            {
                errors["Content"] = new[] { "Nội dung câu hỏi không được để trống." };
            }
            else if (trimmedContent.Length > 2000)
            {
                errors["Content"] = new[] { "Nội dung câu hỏi không được vượt quá 2000 ký tự." };
            }
            else if (subject != null && await _questions.ExistsContentInSubjectAsync(input.SubjectId, trimmedContent, id))
            {
                errors["Content"] = new[] { "Câu hỏi với nội dung này đã tồn tại trong môn học đã chọn." };
            }

            if (input.BloomLevel < 1 || input.BloomLevel > 4)
            {
                errors["BloomLevel"] = new[] { "Mức độ Bloom phải nằm trong khoảng từ 1 đến 4." };
            }

            if (errors.Count > 0)
            {
                return ServiceResult<QuestionDto>.Failure(errors);
            }

            question.SubjectId = input.SubjectId;
            question.Content = trimmedContent;
            question.BloomLevel = input.BloomLevel;
            question.IsActive = input.IsActive;

            await _questions.UpdateAsync(question);

            var updated = await _questions.GetByIdAsync(question.Id) ?? question;
            var dto = updated.ToDto();

            await _notifier.SendToAdminsAsync(HubEvents.QuestionUpdated, dto);
            return ServiceResult<QuestionDto>.Success(dto);
        }

        public async Task<ServiceResult<QuestionDto>> ToggleActiveAsync(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null)
            {
                return ServiceResult<QuestionDto>.Failure(string.Empty, "Không tìm thấy câu hỏi.");
            }

            question.IsActive = !question.IsActive;
            await _questions.UpdateAsync(question);

            var dto = question.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.QuestionUpdated, dto);
            return ServiceResult<QuestionDto>.Success(dto);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var question = await _questions.GetByIdAsync(id);
            if (question == null)
            {
                return ServiceResult.Failure(string.Empty, "Không tìm thấy câu hỏi.");
            }

            if (await _questions.HasBeenUsedInSlotsAsync(id))
            {
                return ServiceResult.Failure(string.Empty, "Câu hỏi đã từng được bốc cho thí sinh dự thi. Hãy khóa câu hỏi thay vì xóa để bảo toàn lịch sử.");
            }

            var deleted = await _questions.TryDeleteAsync(question);
            if (!deleted)
            {
                return ServiceResult.Failure(string.Empty, "Không thể xóa câu hỏi do ràng buộc dữ liệu.");
            }

            await _notifier.SendToAdminsAsync(HubEvents.QuestionDeleted, id);
            return ServiceResult.Success();
        }
    }
}
