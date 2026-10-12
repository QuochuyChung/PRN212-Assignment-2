using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IExamSessionService
    {
        Task<IReadOnlyList<SessionDto>> GetAllAsync(string? search = null, int? subjectId = null, string? status = null);
        Task<IReadOnlyList<SessionDto>> GetUpcomingAsync(int? subjectId = null);
        Task<SessionDto?> GetByIdAsync(int id);
        Task<ServiceResult<SessionDto>> CreateAsync(SessionSaveDto dto);
        Task<ServiceResult<SessionDto>> UpdateAsync(int id, SessionSaveDto dto);
        Task<ServiceResult> DeleteAsync(int id);
        Task<ServiceResult<SessionDto>> CancelAsync(int id);
    }
}
