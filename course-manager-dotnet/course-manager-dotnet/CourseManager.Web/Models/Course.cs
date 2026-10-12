using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Web.Models;

[Table("courses")]
public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên khóa học không được để trống.")]
    [StringLength(150, ErrorMessage = "Tên khóa học tối đa 150 ký tự.")]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "Học phí phải lớn hơn 0.")]
    [Column(TypeName = "decimal(14,2)")]
    public decimal TuitionFee { get; set; }

    [Range(1, 10000, ErrorMessage = "Thời lượng phải lớn hơn 0 giờ.")]
    public int Duration { get; set; }

    [Display(Name = "Đang tuyển sinh")]
    public bool IsActive { get; set; } = true;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    [Display(Name = "Loại khóa học")]
    public int CourseCategoryId { get; set; }
    public CourseCategory? CourseCategory { get; set; }

    public List<CourseImage> Images { get; set; } = [];

    [NotMapped]
    public string DurationLabel => Duration <= 30 ? "Ngắn hạn" : "Dài hạn";
}
