using asg2.BLL.Dtos;
using asg2.DAL.Entities;

namespace asg2.BLL.Mapping
{
    internal static class RoomMapping
    {
        public static RoomDto ToDto(this Room r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Building = r.Building,
            IsActive = r.IsActive
        };
    }
}
