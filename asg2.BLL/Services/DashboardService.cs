using asg2.BLL.Dtos;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class DashboardService : IDashboardService
    {
        private readonly IAccountRepository _accounts;
        private readonly ISubjectRepository _subjects;
        private readonly IRoomRepository _rooms;
        private readonly IExamSessionRepository _sessions;
        private readonly IQuestionRepository _questions;

        public DashboardService(
            IAccountRepository accounts,
            ISubjectRepository subjects,
            IRoomRepository rooms,
            IExamSessionRepository sessions,
            IQuestionRepository questions)
        {
            _accounts = accounts;
            _subjects = subjects;
            _rooms = rooms;
            _sessions = sessions;
            _questions = questions;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var students = await _accounts.GetByRoleAsync(AccountRole.Student, search: null, includeInactive: true);

            return new DashboardStatsDto
            {
                TotalSubjects = await _subjects.CountAsync(),
                TotalRooms = await _rooms.CountAsync(),
                ActiveRooms = await _rooms.CountActiveAsync(),
                TotalStudents = students.Count,
                TotalSessions = await _sessions.CountAsync(),
                TotalQuestions = await _questions.CountAsync()
            };
        }
    }
}
