using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IQuestionService
    {
        Task<IReadOnlyList<QuestionDto>> GetAllAsync(int? subjectId = null, int? bloomLevel = null, string? search = null, bool includeInactive = true);
        Task<QuestionPagedResultDto> GetPagedAsync(int pageIndex, int pageSize, int? subjectId = null, int? bloomLevel = null, string? search = null, bool includeInactive = true);
        Task<QuestionStatsDto> GetStatsAsync(int? subjectId = null);
        Task<QuestionDto?> GetByIdAsync(int id);
        Task<ServiceResult<QuestionDto>> CreateAsync(QuestionSaveDto input);
        Task<ServiceResult<QuestionDto>> UpdateAsync(int id, QuestionSaveDto input);
        Task<ServiceResult<QuestionDto>> ToggleActiveAsync(int id);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
