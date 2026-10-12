using asg2.BLL.Dtos;
using asg2.BLL.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asg2.Web.Pages.Student.Schedule
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

        public int? SubjectId { get; private set; }
        public IReadOnlyList<SubjectDto> Subjects { get; private set; } = Array.Empty<SubjectDto>();
        public IReadOnlyList<SessionDto> Sessions { get; private set; } = Array.Empty<SessionDto>();

        public async Task OnGetAsync(int? subjectId)
        {
            SubjectId = subjectId;
            Subjects = await _subjects.GetAllAsync();
            Sessions = await _sessions.GetUpcomingAsync(subjectId);
        }
    }
}
