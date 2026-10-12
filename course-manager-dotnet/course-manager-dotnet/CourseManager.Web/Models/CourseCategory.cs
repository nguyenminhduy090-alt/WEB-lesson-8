using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Web.Models;

[Table("course_categories")]
public class CourseCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên loại khóa học không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên loại tối đa 100 ký tự.")]
    public string Name { get; set; } = "";

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    public string? Description { get; set; }

    public List<Course> Courses { get; set; } = [];
}
