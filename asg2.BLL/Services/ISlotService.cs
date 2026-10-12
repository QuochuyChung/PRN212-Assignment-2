using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface ISlotService
    {
        Task<IReadOnlyList<SlotDto>> GetSlotsOfSessionAsync(int sessionId);
        Task<IReadOnlyList<SlotDto>> GetMySlotsAsync(int studentId);
        Task<ServiceResult> RecalculateSlotsAsync(int sessionId);
        Task<ServiceResult> RemoveAllOfSessionAsync(int sessionId);
        Task<ServiceResult<IReadOnlyList<SlotDto>>> AssignStudentsAsync(int sessionId, IEnumerable<int> studentIds);
        Task<ServiceResult<SlotDto>> AssignStudentAsync(int sessionId, int studentId);
        Task<ServiceResult> RemoveStudentAsync(int slotId);
    }
}
