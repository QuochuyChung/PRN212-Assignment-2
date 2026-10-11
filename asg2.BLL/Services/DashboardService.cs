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

        public DashboardService(IAccountRepository accounts, ISubjectRepository subjects, IRoomRepository rooms)
        {
            _accounts = accounts;
            _subjects = subjects;
            _rooms = rooms;
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
                // TODO(P3): đếm ExamSession khi entity đã tồn tại (đọc repo của P3, không ghi).
                TotalSessions = 0,
                // TODO(P5): đếm Question khi entity đã tồn tại (đọc repo của P5, không ghi).
                TotalQuestions = 0
            };
        }
    }
}
