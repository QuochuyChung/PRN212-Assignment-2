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

        public DashboardService(
            IAccountRepository accounts,
            ISubjectRepository subjects,
            IRoomRepository rooms,
            IExamSessionRepository sessions)
        {
            _accounts = accounts;
            _subjects = subjects;
            _rooms = rooms;
            _sessions = sessions;
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
                // TODO(P5): đếm Question khi entity đã tồn tại (đọc repo của P5, không ghi).
                TotalQuestions = 0
            };
        }
    }
}
