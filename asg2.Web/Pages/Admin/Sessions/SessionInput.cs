using System.ComponentModel.DataAnnotations;
using asg2.BLL.Dtos;

namespace asg2.Web.Pages.Admin.Sessions
{
    public class SessionInput
    {
        [Required(ErrorMessage = "Tiêu đề phiên thi là bắt buộc.")]
        [StringLength(150, ErrorMessage = "Tiêu đề tối đa 150 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học.")]
        [Display(Name = "Môn học")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng thi.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng thi.")]
        [Display(Name = "Phòng thi")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(1).AddHours(8);

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1).AddHours(11);

        [Required(ErrorMessage = "Thời lượng slot là bắt buộc.")]
        [Range(5, 180, ErrorMessage = "Thời lượng mỗi slot từ 5 đến 180 phút.")]
        [Display(Name = "Thời lượng mỗi slot (phút)")]
        public int SlotMinutes { get; set; } = 15;

        [Required(ErrorMessage = "Số câu hỏi chính là bắt buộc.")]
        [Range(1, 20, ErrorMessage = "Số câu hỏi chính từ 1 đến 20.")]
        [Display(Name = "Số câu hỏi chính")]
        public int MainQuestionCount { get; set; } = 2;

        [Range(0, 10, ErrorMessage = "Số câu hỏi phụ tối đa từ 0 đến 10.")]
        [Display(Name = "Số câu hỏi phụ tối đa")]
        public int MaxFollowUpCount { get; set; } = 1;

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Scheduled";

        public SessionSaveDto ToDto() => new()
        {
            Title = Title,
            SubjectId = SubjectId,
            RoomId = RoomId,
            StartTime = StartTime,
            EndTime = EndTime,
            SlotMinutes = SlotMinutes,
            MainQuestionCount = MainQuestionCount,
            MaxFollowUpCount = MaxFollowUpCount,
            Status = Status
        };

        public static SessionInput From(SessionDto dto) => new()
        {
            Title = dto.Title,
            SubjectId = dto.SubjectId,
            RoomId = dto.RoomId,
            StartTime = dto.StartTime.ToLocalTime(),
            EndTime = dto.EndTime.ToLocalTime(),
            SlotMinutes = dto.SlotMinutes,
            MainQuestionCount = dto.MainQuestionCount,
            MaxFollowUpCount = dto.MaxFollowUpCount,
            Status = dto.Status
        };
    }
}
