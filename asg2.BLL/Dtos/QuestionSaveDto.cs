using System.ComponentModel.DataAnnotations;

namespace asg2.BLL.Dtos
{
    public class QuestionSaveDto
    {
        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
        [StringLength(2000, ErrorMessage = "Nội dung câu hỏi tối đa 2000 ký tự.")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Mức độ Bloom phải từ 1 đến 4.")]
        public int BloomLevel { get; set; } = 1;

        public bool IsActive { get; set; } = true;
    }
}
