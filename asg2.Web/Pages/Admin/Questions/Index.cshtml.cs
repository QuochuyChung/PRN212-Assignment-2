using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Questions
{
    public class IndexModel : PageModel
    {
        private readonly IQuestionService _questions;
        private readonly ISubjectService _subjects;

        public IndexModel(
            IQuestionService questions,
            ISubjectService subjects)
        {
            _questions = questions;
            _subjects = subjects;
        }

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();
        public QuestionPagedResultDto PagedResult { get; private set; } = new();
        public QuestionStatsDto Stats { get; private set; } = new();

        public int? SubjectId { get; private set; }
        public int? BloomLevel { get; private set; }
        public string? Q { get; private set; }
        public int PageNumber { get; private set; } = 1;

        public async Task OnGetAsync(int? subjectId, int? bloomLevel, string? q, int p = 1)
        {
            SubjectId = subjectId;
            BloomLevel = bloomLevel;
            Q = q;
            PageNumber = p < 1 ? 1 : p;

            Subjects = await _subjects.GetAllAsync();
            Stats = await _questions.GetStatsAsync(subjectId);
            PagedResult = await _questions.GetPagedAsync(PageNumber, 10, subjectId, bloomLevel, q, includeInactive: true);
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id, int p = 1)
        {
            var result = await _questions.ToggleActiveAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Errors.TryGetValue(string.Empty, out var msgs)
                    ? msgs[0]
                    : "Không thể thay đổi trạng thái câu hỏi.";
            }
            else
            {
                var status = result.Value!.IsActive ? "kích hoạt" : "khóa";
                TempData["Success"] = $"Đã {status} câu hỏi #{id}.";
            }

            return RedirectToPage(new { subjectId = SubjectId, bloomLevel = BloomLevel, q = Q, p });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int p = 1)
        {
            var result = await _questions.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Errors.TryGetValue(string.Empty, out var msgs)
                    ? msgs[0]
                    : "Không thể xóa câu hỏi.";
            }
            else
            {
                TempData["Success"] = $"Đã xóa câu hỏi #{id}.";
            }

            return RedirectToPage(new { subjectId = SubjectId, bloomLevel = BloomLevel, q = Q, p });
        }
    }
}
