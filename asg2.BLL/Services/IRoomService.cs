using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface IRoomService
    {
        /// <summary>Toàn bộ phòng (kể cả ngừng dùng), sắp theo tên. Trang Admin/Rooms dùng.</summary>
        Task<IReadOnlyList<RoomDto>> GetAllAsync();

        /// <summary>Phòng theo từ khóa (tên hoặc tòa nhà, không phân biệt hoa thường). Từ khóa rỗng thì trả toàn bộ.</summary>
        Task<IReadOnlyList<RoomDto>> SearchAsync(string? keyword);

        /// <summary>
        /// Chỉ phòng còn hoạt động — P3 dùng khi chọn phòng cho phiên thi. Truyền <paramref name="includeRoomId"/>
        /// để giữ lại phòng hiện tại của phiên thi kể cả khi phòng đó đã bị khóa (trang sửa phiên thi).
        /// </summary>
        Task<IReadOnlyList<RoomDto>> GetActiveAsync(int? includeRoomId = null);

        Task<RoomDto?> GetByIdAsync(int id);

        Task<ServiceResult<RoomDto>> CreateAsync(RoomSaveDto dto);

        Task<ServiceResult<RoomDto>> UpdateAsync(int id, RoomSaveDto dto);

        /// <summary>Xóa phòng. Bị chặn khi còn phiên thi đang dùng.</summary>
        Task<ServiceResult> DeleteAsync(int id);
    }
}
