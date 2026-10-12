using System.ComponentModel.DataAnnotations;

namespace CourseManager.Web.ViewModels;

public class CourseFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên khóa học không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên khóa học tối đa 150 ký tự.")]
    [Display(Name = "Tên khóa học")]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Học phí phải lớn hơn 0.")]
    [Display(Name = "Học phí (VNĐ)")]
    public decimal TuitionFee { get; set; }

    [Range(1, 10000, ErrorMessage = "Thời lượng phải lớn hơn 0 giờ.")]
    [Display(Name = "Thời lượng (giờ)")]
    public int Duration { get; set; }

    [Display(Name = "Trạng thái tuyển sinh")]
    public bool IsActive { get; set; } = true;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Vui lòng nhập URL ảnh hợp lệ, bắt đầu bằng http:// hoặc https://.")]
    [StringLength(1000, ErrorMessage = "URL ảnh tối đa 1000 ký tự.")]
    [Display(Name = "Link ảnh đại diện (không bắt buộc)")]
    public string? ImageUrl { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại khóa học.")]
    [Display(Name = "Loại khóa học")]
    public int CourseCategoryId { get; set; }
}
