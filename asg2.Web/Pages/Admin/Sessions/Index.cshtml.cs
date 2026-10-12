using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class IndexModel : PageModel
    {
        private readonly IExamSessionService _sessions;
        private readonly ISubjectService _subjects;

        public IndexModel(IExamSessionService sessions, ISubjectService subjects)
        {
            _sessions = sessions;
            _subjects = subjects;
        }

        public string? Q { get; private set; }
        public int? SubjectId { get; private set; }
        public string? Status { get; private set; }

        public IReadOnlyList<SessionDto> Sessions { get; private set; } = Array.Empty<SessionDto>();
        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();

        public async Task OnGetAsync(string? q, int? subjectId, string? status)
        {
            Q = q;
            SubjectId = subjectId;
            Status = status;

            Subjects = await _subjects.GetAllAsync();
            Sessions = await _sessions.GetAllAsync(q, subjectId, status);
        }
    }
}
