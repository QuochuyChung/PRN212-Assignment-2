using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class QuestionPickerService : IQuestionPickerService
    {
        private readonly IExamSlotRepository _slots;
        private readonly IExamSessionRepository _sessions;
        private readonly IQuestionRepository _questions;
        private readonly ISlotQuestionRepository _slotQuestions;
        private readonly IRealtimeNotifier _notifier;

        public QuestionPickerService(
            IExamSlotRepository slots,
            IExamSessionRepository sessions,
            IQuestionRepository questions,
            ISlotQuestionRepository slotQuestions,
            IRealtimeNotifier notifier)
        {
            _slots = slots;
            _sessions = sessions;
            _questions = questions;
            _slotQuestions = slotQuestions;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<SlotQuestionDto>> GetQuestionsOfSlotAsync(int slotId)
        {
            var list = await _slotQuestions.GetBySlotIdAsync(slotId);
            return list.Select(sq => sq.ToDto()).ToList();
        }

        public async Task<ServiceResult<IReadOnlyList<SlotQuestionDto>>> PickForSlotAsync(int slotId)
        {
            var slot = await _slots.GetByIdAsync(slotId);
            if (slot == null)
            {
                return ServiceResult<IReadOnlyList<SlotQuestionDto>>.Failure(string.Empty, "Không tìm thấy ca thi.");
            }

            var session = await _sessions.GetByIdAsync(slot.SessionId);
            if (session == null)
            {
                return ServiceResult<IReadOnlyList<SlotQuestionDto>>.Failure(string.Empty, "Không tìm thấy phiên thi của ca này.");
            }

            var pool = await _questions.GetActiveBySubjectIdAsync(session.SubjectId);
            if (pool.Count == 0)
            {
                return ServiceResult<IReadOnlyList<SlotQuestionDto>>.Failure(
                    string.Empty,
                    $"Ngân hàng câu hỏi của môn {session.Subject?.Code ?? "này"} chưa có câu hỏi nào đang hoạt động.");
            }

            var allSlots = await _slots.GetBySessionIdAsync(slot.SessionId);
            var prevSlot = allSlots
                .Where(s => s.OrderNo < slot.OrderNo)
                .OrderByDescending(s => s.OrderNo)
                .FirstOrDefault();

            var prevQuestionIds = new HashSet<int>();
            if (prevSlot != null)
            {
                var prevQuestions = await _slotQuestions.GetBySlotIdAsync(prevSlot.Id);
                foreach (var pq in prevQuestions)
                {
                    prevQuestionIds.Add(pq.QuestionId);
                }
            }

            int count = Math.Max(1, session.MainQuestionCount);
            if (count > pool.Count)
            {
                count = pool.Count;
            }

            var selectedQuestions = PickRandomQuestionsAvoidingPrevious(pool, prevQuestionIds, count);

            await _slotQuestions.RemoveBySlotIdAsync(slotId);

            var newSlotQuestions = selectedQuestions.Select((q, idx) => new SlotQuestion
            {
                SlotId = slotId,
                QuestionId = q.Id,
                OrderNo = idx + 1
            }).ToList();

            await _slotQuestions.AddRangeAsync(newSlotQuestions);

            var updatedList = await _slotQuestions.GetBySlotIdAsync(slotId);
            var dtos = updatedList.Select(sq => sq.ToDto()).ToList();

            var payload = new
            {
                slotId = slot.Id,
                sessionId = slot.SessionId,
                questions = dtos
            };

            await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotQuestionsUpdated, payload);
            await _notifier.SendToAdminsAsync(HubEvents.SlotQuestionsUpdated, payload);

            return ServiceResult<IReadOnlyList<SlotQuestionDto>>.Success(dtos);
        }

        public async Task<ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>> PickForAllSlotsInSessionAsync(int sessionId)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null)
            {
                return ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>.Failure(string.Empty, "Không tìm thấy phiên thi.");
            }

            var allSlots = (await _slots.GetBySessionIdAsync(sessionId))
                .OrderBy(s => s.OrderNo)
                .ToList();

            if (allSlots.Count == 0)
            {
                var emptyResult = new Dictionary<int, IReadOnlyList<SlotQuestionDto>>();
                return ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>.Success(emptyResult);
            }

            var pool = await _questions.GetActiveBySubjectIdAsync(session.SubjectId);
            if (pool.Count == 0)
            {
                return ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>.Failure(
                    string.Empty,
                    $"Ngân hàng câu hỏi của môn {session.Subject?.Code ?? "này"} chưa có câu hỏi nào đang hoạt động.");
            }

            int count = Math.Max(1, session.MainQuestionCount);
            if (count > pool.Count)
            {
                count = pool.Count;
            }

            await _slotQuestions.RemoveBySessionIdAsync(sessionId);

            var resultMap = new Dictionary<int, IReadOnlyList<SlotQuestionDto>>();
            var prevQuestionIds = new HashSet<int>();
            var newRecords = new List<SlotQuestion>();

            foreach (var slot in allSlots)
            {
                var chosen = PickRandomQuestionsAvoidingPrevious(pool, prevQuestionIds, count);

                newRecords.AddRange(chosen.Select((q, idx) => new SlotQuestion
                {
                    SlotId = slot.Id,
                    QuestionId = q.Id,
                    OrderNo = idx + 1
                }));

                // Cập nhật lại prevQuestionIds cho slot kế tiếp
                prevQuestionIds = chosen.Select(q => q.Id).ToHashSet();
            }

            await _slotQuestions.AddRangeAsync(newRecords);

            var allAssigned = await _slotQuestions.GetBySessionIdAsync(sessionId);
            var grouped = allAssigned.GroupBy(sq => sq.SlotId);

            foreach (var group in grouped)
            {
                var dtos = group.OrderBy(sq => sq.OrderNo).Select(sq => sq.ToDto()).ToList();
                resultMap[group.Key] = dtos;

                var slot = allSlots.FirstOrDefault(s => s.Id == group.Key);
                if (slot != null)
                {
                    var payload = new
                    {
                        slotId = slot.Id,
                        sessionId = session.Id,
                        questions = dtos
                    };
                    await _notifier.SendToUserAsync(slot.StudentId, HubEvents.SlotQuestionsUpdated, payload);
                    await _notifier.SendToAdminsAsync(HubEvents.SlotQuestionsUpdated, payload);
                }
            }

            return ServiceResult<IReadOnlyDictionary<int, IReadOnlyList<SlotQuestionDto>>>.Success(resultMap);
        }

        /// <summary>
        /// Thuật toán bốc ngẫu nhiên count câu hỏi, ưu tiên hoàn toàn các câu chưa xuất hiện ở slot trước.
        /// Khi ngân hàng đủ câu (>= 2 * count), đảm bảo 0% trùng lặp giữa 2 slot liền kề.
        /// Khi ngân hàng thiếu câu, đảm bảo trùng lặp tối thiểu (ít nhất có thể).
        /// </summary>
        private static List<Question> PickRandomQuestionsAvoidingPrevious(
            IReadOnlyList<Question> pool,
            HashSet<int> prevQuestionIds,
            int count)
        {
            var nonOverlapping = pool.Where(q => !prevQuestionIds.Contains(q.Id)).ToList();
            var overlapping = pool.Where(q => prevQuestionIds.Contains(q.Id)).ToList();

            // Xáo trộn ngẫu nhiên (Fisher-Yates)
            Shuffle(nonOverlapping);
            Shuffle(overlapping);

            var result = new List<Question>();

            if (nonOverlapping.Count >= count)
            {
                // Ngân hàng đủ câu không trùng: lấy hoàn toàn từ nonOverlapping
                result.AddRange(nonOverlapping.Take(count));
            }
            else
            {
                // Ngân hàng không đủ câu: lấy hết nonOverlapping, bù phần còn thiếu từ overlapping
                result.AddRange(nonOverlapping);
                int needed = count - result.Count;
                result.AddRange(overlapping.Take(needed));
            }

            return result;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
