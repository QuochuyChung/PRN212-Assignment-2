using System.ComponentModel.DataAnnotations;
using asg2.BLL.Dtos;

namespace asg2.Web.Pages.Admin.Questions
{
    public class QuestionInput
    {
        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        [Display(Name = "Môn học")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
        [StringLength(2000, ErrorMessage = "Nội dung câu hỏi tối đa 2000 ký tự.")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Mức độ Bloom phải từ 1 đến 4.")]
        [Display(Name = "Mức độ tư duy (Bloom)")]
        public int BloomLevel { get; set; } = 1;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public QuestionSaveDto ToDto() => new()
        {
            SubjectId = SubjectId,
            Content = Content,
            BloomLevel = BloomLevel,
            IsActive = IsActive
        };

        public static QuestionInput FromDto(QuestionDto dto) => new()
        {
            SubjectId = dto.SubjectId,
            Content = dto.Content,
            BloomLevel = dto.BloomLevel,
            IsActive = dto.IsActive
        };
    }
}
