using System.ComponentModel.DataAnnotations;
using asg2.BLL.Dtos;

namespace asg2.Web.Pages.Admin.Subjects
{
    /// <summary>Dữ liệu form tạo / sửa môn học. Validate đầy đủ nằm ở SubjectService (BLL); đây chỉ là kiểm tra nhanh phía form.</summary>
    public class SubjectInput
    {
        [Required(ErrorMessage = "Mã môn là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã môn tối đa 20 ký tự.")]
        [Display(Name = "Mã môn")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên môn học là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên môn học tối đa 100 ký tự.")]
        [Display(Name = "Tên môn học")]
        public string Name { get; set; } = string.Empty;

        public SubjectSaveDto ToDto() => new()
        {
            Code = Code,
            Name = Name
        };

        public static SubjectInput From(SubjectDto dto) => new()
        {
            Code = dto.Code,
            Name = dto.Name
        };
    }
}
