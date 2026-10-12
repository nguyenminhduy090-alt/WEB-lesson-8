using CourseManager.Web.Models;

namespace CourseManager.Web.ViewModels;

public class CourseListViewModel
{
    public List<Course> Courses { get; set; } = [];
    public List<CourseCategory> Categories { get; set; } = [];
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool? Active { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public string Sort { get; set; } = "";
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string? ErrorMessage { get; set; }
}
