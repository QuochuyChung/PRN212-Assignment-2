using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IQuestionPickerService
    {
        Task<IReadOnlyList<SlotQuestionDto>> GetQuestionsOfSlotAsync(int slotId);
        Task<ServiceResult<IReadOnlyList<SlotQuestionDto>>> PickForSlotAsync(int slotId);
        Task<ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>> PickForAllSlotsInSessionAsync(int sessionId);
    }
}
