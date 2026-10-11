using System.ComponentModel.DataAnnotations;
using asg2.BLL.Dtos;

namespace asg2.Web.Pages.Admin.Rooms
{
    /// <summary>Dữ liệu form tạo / sửa phòng thi. Validate đầy đủ nằm ở RoomService (BLL); đây chỉ là kiểm tra nhanh phía form.</summary>
    public class RoomInput
    {
        [Required(ErrorMessage = "Tên phòng là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên phòng tối đa 50 ký tự.")]
        [Display(Name = "Tên phòng")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Tên tòa nhà tối đa 50 ký tự.")]
        [Display(Name = "Tòa nhà")]
        public string? Building { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public RoomSaveDto ToDto() => new()
        {
            Name = Name,
            Building = Building,
            IsActive = IsActive
        };

        public static RoomInput From(RoomDto dto) => new()
        {
            Name = dto.Name,
            Building = dto.Building,
            IsActive = dto.IsActive
        };
    }
}
