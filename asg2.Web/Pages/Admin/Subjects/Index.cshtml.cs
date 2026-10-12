using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Subjects
{
    public class IndexModel : PageModel
    {
        private readonly ISubjectService _subjects;

        public IndexModel(ISubjectService subjects)
        {
            _subjects = subjects;
        }

        public string? Q { get; private set; }

        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();

        public async Task OnGetAsync(string? q)
        {
            Q = q;
            var all = await _subjects.GetAllAsync();
            if (string.IsNullOrWhiteSpace(q))
            {
                Subjects = all;
                return;
            }

            var term = q.Trim();
            Subjects = all
                .Where(s => s.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || s.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
