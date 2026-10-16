using asg2.BLL.Common;
using asg2.BLL.Dtos;

namespace asg2.BLL.Services
{
    public interface ISubjectService
    {
        /// <summary>Toàn bộ môn học, sắp theo mã môn. P3/P5 dùng cho dropdown.</summary>
        Task<IReadOnlyList<SubjectDto>> GetAllAsync();

        /// <summary>Môn học theo từ khóa (mã hoặc tên, không phân biệt hoa thường). Từ khóa rỗng thì trả toàn bộ.</summary>
        Task<IReadOnlyList<SubjectDto>> SearchAsync(string? keyword);

        Task<SubjectDto?> GetByIdAsync(int id);

        Task<ServiceResult<SubjectDto>> CreateAsync(SubjectSaveDto dto);

        Task<ServiceResult<SubjectDto>> UpdateAsync(int id, SubjectSaveDto dto);

        /// <summary>Xóa môn học. Bị chặn khi còn phiên thi đang dùng.</summary>
        Task<ServiceResult> DeleteAsync(int id);
    }
}
