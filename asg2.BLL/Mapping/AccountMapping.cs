using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class AccountMapping
    {
        public static AccountDto ToDto(this Account a) => new()
        {
            Id = a.Id,
            Username = a.Username,
            FullName = a.FullName,
            Role = a.Role.ToString(),
            IsActive = a.IsActive
        };
    }
}
