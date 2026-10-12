using System.ComponentModel.DataAnnotations;

namespace CourseManager.Web.ViewModels;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên loại khóa học không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên loại tối đa 100 ký tự.")]
    [Display(Name = "Tên loại khóa học")]
    public string Name { get; set; } = "";

    [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
    public string? Description { get; set; }
}
