using System.ComponentModel.DataAnnotations;

namespace CourseManager.Web.ViewModels;

public class CourseImageFormViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập URL ảnh.")]
    [StringLength(1000, ErrorMessage = "URL ảnh tối đa 1000 ký tự.")]
    [Url(ErrorMessage = "URL ảnh không hợp lệ.")]
    [Display(Name = "URL ảnh")]
    public string ImageUrl { get; set; } = "";
}
