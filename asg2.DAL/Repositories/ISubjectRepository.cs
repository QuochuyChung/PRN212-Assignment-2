using asg2.DAL.Entities;

namespace asg2.DAL.Repositories
{
    public interface ISubjectRepository
    {
        Task<IReadOnlyList<Subject>> GetAllAsync();
        Task<Subject?> GetByIdAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<int> CountAsync();
        Task AddAsync(Subject subject);
        Task UpdateAsync(Subject subject);

        /// <summary>Xóa môn học. Trả về false nếu bị ràng buộc khóa ngoại (đang được bảng khác tham chiếu).</summary>
        Task<bool> TryDeleteAsync(Subject subject);

        /// <summary>Môn học có đang được bảng khác (phiên thi) dùng không — service gọi trước khi xóa.</summary>
        Task<bool> IsInUseAsync(int id);
    }
}
