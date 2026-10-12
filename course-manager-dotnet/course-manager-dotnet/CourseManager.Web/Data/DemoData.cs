using CourseManager.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Web.Data;

public static class DemoData
{
    public static async Task SeedAsync(CourseDbContext db)
    {
        if (await db.CourseCategories.AnyAsync() || await db.Courses.AnyAsync()) return;

        var web = new CourseCategory { Name = "Lập trình Web", Description = "Frontend và backend web" };
        var data = new CourseCategory { Name = "Khoa học dữ liệu", Description = "Phân tích và xử lý dữ liệu" };
        var design = new CourseCategory { Name = "Thiết kế", Description = "Thiết kế giao diện và trải nghiệm" };
        db.CourseCategories.AddRange(web, data, design);
        await db.SaveChangesAsync();

        var courses = new[]
        {
            Sample("Spring Boot căn bản", 2500000m, 24, true, "Xây dựng ứng dụng web với Spring Boot và JPA.", web,
                "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=900"),
            Sample("ReactJS thực chiến", 3200000m, 36, true, "Phát triển giao diện web hiện đại với React.", web,
                "https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=900"),
            Sample("SQL và MySQL", 1800000m, 18, true, "Thiết kế cơ sở dữ liệu và truy vấn SQL.", data,
                "https://images.unsplash.com/photo-1518770660439-4636190af475?w=900"),
            Sample("Phân tích dữ liệu với Python", 4500000m, 48, false, "Làm sạch và phân tích dữ liệu.", data,
                "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=900"),
            Sample("UI/UX căn bản", 2100000m, 30, true, "Nguyên tắc thiết kế giao diện lấy người dùng làm trung tâm.", design,
                "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=900")
        };
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();
    }

    private static Course Sample(string name, decimal fee, int hours, bool active, string description,
        CourseCategory category, string imageUrl)
    {
        var course = new Course
        {
            Name = name, TuitionFee = fee, Duration = hours, IsActive = active,
            Description = description, CourseCategoryId = category.Id, CourseCategory = category
        };
        course.Images.Add(new CourseImage { ImageUrl = imageUrl, IsThumbnail = true });
        return course;
    }
}
