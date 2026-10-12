using CourseManager.Web.Data;
using CourseManager.Web.Models;
using CourseManager.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Web.Services;

public class CategoryService(CourseDbContext db)
{
    public async Task<List<CategoryRow>> ListAsync() => await db.CourseCategories.AsNoTracking()
        .OrderBy(category => category.Name)
        .Select(category => new CategoryRow
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CourseCount = category.Courses.Count
        }).ToListAsync();

    public Task<List<CourseCategory>> ChoicesAsync() => db.CourseCategories.AsNoTracking()
        .OrderBy(category => category.Name).ToListAsync();

    public Task<CourseCategory?> FindAsync(int id) => db.CourseCategories.FindAsync(id).AsTask();

    public async Task<string?> SaveAsync(CategoryFormViewModel form)
    {
        var name = form.Name.Trim();
        var duplicate = await db.CourseCategories.AnyAsync(category =>
            category.Name.ToLower() == name.ToLower() && (!form.Id.HasValue || category.Id != form.Id.Value));
        if (duplicate) return "Tên loại khóa học đã được sử dụng.";

        CourseCategory category;
        if (form.Id.HasValue)
            category = await db.CourseCategories.FindAsync(form.Id.Value) ?? throw new KeyNotFoundException("Không tìm thấy loại khóa học.");
        else
        {
            category = new CourseCategory();
            db.CourseCategories.Add(category);
        }
        category.Name = name;
        category.Description = form.Description?.Trim();
        await db.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteAsync(int id)
    {
        var category = await db.CourseCategories.FindAsync(id);
        if (category is null) throw new KeyNotFoundException("Không tìm thấy loại khóa học.");
        if (await db.Courses.AnyAsync(course => course.CourseCategoryId == id))
            return "Không thể xóa loại đang có khóa học. Hãy chuyển hoặc xóa các khóa học trước.";
        db.CourseCategories.Remove(category);
        await db.SaveChangesAsync();
        return null;
    }
}
