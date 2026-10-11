using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface IRoomRepository
    {
        Task<IReadOnlyList<Room>> GetAllAsync();
        Task<IReadOnlyList<Room>> GetActiveAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> CountAsync();
        Task<int> CountActiveAsync();
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);

        /// <summary>Xóa phòng. Trả về false nếu bị ràng buộc khóa ngoại (đang được bảng khác tham chiếu).</summary>
        Task<bool> TryDeleteAsync(Room room);

        /// <summary>Phòng có đang được bảng khác (phiên thi) dùng không — service gọi trước khi xóa.</summary>
        Task<bool> IsInUseAsync(int id);
    }
}
