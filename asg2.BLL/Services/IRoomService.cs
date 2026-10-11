using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IRoomService
    {
        /// <summary>Toàn bộ phòng (kể cả ngừng dùng), sắp theo tên. Trang Admin/Rooms dùng.</summary>
        Task<IReadOnlyList<RoomDto>> GetAllAsync();

        /// <summary>Chỉ phòng còn hoạt động — P3 dùng khi chọn phòng cho phiên thi.</summary>
        Task<IReadOnlyList<RoomDto>> GetActiveAsync();

        Task<RoomDto?> GetByIdAsync(int id);

        Task<ServiceResult<RoomDto>> CreateAsync(RoomSaveDto dto);

        Task<ServiceResult<RoomDto>> UpdateAsync(int id, RoomSaveDto dto);

        /// <summary>Xóa phòng. Bị chặn khi còn phiên thi đang dùng.</summary>
        Task<ServiceResult> DeleteAsync(int id);
    }
}
