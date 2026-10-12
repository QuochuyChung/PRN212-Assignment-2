using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class SubjectMapping
    {
        public static SubjectDto ToDto(this Subject s) => new()
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name
        };
    }
}
