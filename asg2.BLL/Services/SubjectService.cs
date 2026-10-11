using System.Text.RegularExpressions;
using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class SubjectService : ISubjectService
    {
        private static readonly Regex CodePattern = new(@"^[A-Za-z0-9\-]{2,20}$", RegexOptions.Compiled);

        private readonly ISubjectRepository _subjects;
        private readonly IRealtimeNotifier _notifier;

        public SubjectService(ISubjectRepository subjects, IRealtimeNotifier notifier)
        {
            _subjects = subjects;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<SubjectDto>> GetAllAsync()
        {
            var list = await _subjects.GetAllAsync();
            return list.Select(s => s.ToDto()).ToList();
        }

        public async Task<SubjectDto?> GetByIdAsync(int id)
        {
            var subject = await _subjects.GetByIdAsync(id);
            return subject?.ToDto();
        }

        public async Task<ServiceResult<SubjectDto>> CreateAsync(SubjectSaveDto dto)
        {
            var errors = Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<SubjectDto>.Failure(errors);

            var code = dto.Code.Trim();
            if (await _subjects.CodeExistsAsync(code))
                return ServiceResult<SubjectDto>.Failure(nameof(dto.Code), "Mã môn đã tồn tại.");

            var subject = new Subject
            {
                Code = code,
                Name = dto.Name.Trim()
            };
            await _subjects.AddAsync(subject);

            var result = subject.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.SubjectCreated, result);
            return ServiceResult<SubjectDto>.Success(result);
        }

        public async Task<ServiceResult<SubjectDto>> UpdateAsync(int id, SubjectSaveDto dto)
        {
            var subject = await _subjects.GetByIdAsync(id);
            if (subject is null)
                return ServiceResult<SubjectDto>.Failure(string.Empty, "Không tìm thấy môn học.");

            var errors = Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<SubjectDto>.Failure(errors);

            var code = dto.Code.Trim();
            if (await _subjects.CodeExistsAsync(code, id))
                return ServiceResult<SubjectDto>.Failure(nameof(dto.Code), "Mã môn đã tồn tại.");

            subject.Code = code;
            subject.Name = dto.Name.Trim();
            await _subjects.UpdateAsync(subject);

            var result = subject.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.SubjectUpdated, result);
            return ServiceResult<SubjectDto>.Success(result);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var subject = await _subjects.GetByIdAsync(id);
            if (subject is null)
                return ServiceResult.Failure(string.Empty, "Không tìm thấy môn học.");

            if (await _subjects.IsInUseAsync(id))
                return ServiceResult.Failure(string.Empty,
                    "Môn học đang được sử dụng bởi các phiên thi nên không thể xóa.");

            if (!await _subjects.TryDeleteAsync(subject))
                return ServiceResult.Failure(string.Empty,
                    "Môn học đang được sử dụng nên không thể xóa.");

            await _notifier.SendToAdminsAsync(HubEvents.SubjectDeleted, id);
            return ServiceResult.Success();
        }

        private static Dictionary<string, string[]> Validate(SubjectSaveDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            var code = (dto.Code ?? string.Empty).Trim();
            if (code.Length == 0)
                errors[nameof(dto.Code)] = new[] { "Mã môn là bắt buộc." };
            else if (!CodePattern.IsMatch(code))
                errors[nameof(dto.Code)] = new[] { "Mã môn 2-20 ký tự, chỉ gồm chữ, số và dấu gạch ngang." };

            var name = (dto.Name ?? string.Empty).Trim();
            if (name.Length == 0)
                errors[nameof(dto.Name)] = new[] { "Tên môn học là bắt buộc." };
            else if (name.Length > 100)
                errors[nameof(dto.Name)] = new[] { "Tên môn học tối đa 100 ký tự." };

            return errors;
        }
    }
}
