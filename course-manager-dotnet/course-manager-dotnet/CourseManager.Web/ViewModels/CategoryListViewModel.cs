namespace CourseManager.Web.ViewModels;

public class CategoryListViewModel
{
    public List<CategoryRow> Categories { get; set; } = [];
}

public class CategoryRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int CourseCount { get; set; }
}
