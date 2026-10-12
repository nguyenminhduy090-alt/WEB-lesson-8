using CourseManager.Web.Data;
using CourseManager.Web.Models;
using CourseManager.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Web.Services;

public class CourseService(CourseDbContext db)
{
    private const int PageSize = 10;

    public async Task<CourseListViewModel> SearchAsync(CourseListViewModel filter)
    {
        var query = db.Courses.AsNoTracking()
            .Include(course => course.CourseCategory)
            .Include(course => course.Images.Where(image => image.IsThumbnail))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(course => EF.Functions.Like(course.Name, $"%{filter.Search.Trim()}%"));
        if (filter.CategoryId.HasValue)
            query = query.Where(course => course.CourseCategoryId == filter.CategoryId.Value);
        if (filter.Active.HasValue)
            query = query.Where(course => course.IsActive == filter.Active.Value);
        if (filter.MinFee.HasValue)
            query = query.Where(course => course.TuitionFee >= filter.MinFee.Value);
        if (filter.MaxFee.HasValue)
            query = query.Where(course => course.TuitionFee <= filter.MaxFee.Value);

        query = filter.Sort switch
        {
            "feeAsc" => query.OrderBy(course => course.TuitionFee),
            "feeDesc" => query.OrderByDescending(course => course.TuitionFee),
            "durationAsc" => query.OrderBy(course => course.Duration),
            _ => query.OrderByDescending(course => course.Id)
        };

        var total = await query.CountAsync();
        var page = Math.Max(0, filter.Page);
        var courses = await query.Skip(page * PageSize).Take(PageSize).ToListAsync();
        filter.Courses = courses;
        filter.Categories = await db.CourseCategories.AsNoTracking().OrderBy(category => category.Name).ToListAsync();
        filter.TotalItems = total;
        filter.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        filter.Page = page;
        return filter;
    }

    public Task<Course?> FindAsync(int id) => db.Courses
        .Include(course => course.CourseCategory)
        .Include(course => course.Images.OrderBy(image => image.Id))
        .FirstOrDefaultAsync(course => course.Id == id);

    public async Task<string?> SaveAsync(CourseFormViewModel form)
    {
        var imageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl.Trim();
        if (imageUrl is not null &&
            (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri) ||
             (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps)))
            return "Link ảnh phải là địa chỉ http:// hoặc https:// hợp lệ.";

        var categoryExists = await db.CourseCategories.AnyAsync(category => category.Id == form.CourseCategoryId);
        if (!categoryExists) return "Loại khóa học không tồn tại. Hãy chọn lại loại.";

        var normalizedName = form.Name.Trim();
        var duplicate = await db.Courses.AnyAsync(course =>
            course.CourseCategoryId == form.CourseCategoryId &&
            course.Name.ToLower() == normalizedName.ToLower() &&
            (!form.Id.HasValue || course.Id != form.Id.Value));
        if (duplicate) return "Tên khóa học đã có trong loại khóa học này.";

        Course course;
        if (form.Id.HasValue)
        {
            course = await db.Courses.FindAsync(form.Id.Value) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        }
        else
        {
            course = new Course();
            db.Courses.Add(course);
        }

        course.Name = normalizedName;
        course.TuitionFee = form.TuitionFee;
        course.Duration = form.Duration;
        course.IsActive = form.IsActive;
        course.Description = form.Description?.Trim();
        course.CourseCategoryId = form.CourseCategoryId;
        await db.SaveChangesAsync();

        // If an image URL was entered in the course form, add it to the gallery
        // and make it the selected thumbnail. A blank field leaves the current
        // thumbnail unchanged.
        if (imageUrl is not null)
        {
            var currentThumbnails = await db.CourseImages
                .Where(image => image.CourseId == course.Id && image.IsThumbnail)
                .ToListAsync();
            foreach (var current in currentThumbnails)
                current.IsThumbnail = false;

            db.CourseImages.Add(new CourseImage
            {
                CourseId = course.Id,
                ImageUrl = imageUrl,
                IsThumbnail = true
            });
            await db.SaveChangesAsync();
        }
        return null;
    }

    public async Task DeleteAsync(int id)
    {
        var course = await db.Courses.FindAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        db.Courses.Remove(course);
        await db.SaveChangesAsync();
    }

    public async Task<string?> AddImageAsync(int courseId, string url)
    {
        if (!await db.Courses.AnyAsync(course => course.Id == courseId)) return null;
        var hasImages = await db.CourseImages.AnyAsync(image => image.CourseId == courseId);
        db.CourseImages.Add(new CourseImage
        {
            CourseId = courseId,
            ImageUrl = url.Trim(),
            IsThumbnail = !hasImages
        });
        await db.SaveChangesAsync();
        return "Đã thêm ảnh. " + (!hasImages ? "Ảnh đầu tiên được đặt làm ảnh đại diện." : "");
    }

    public async Task<bool> ChooseThumbnailAsync(int courseId, int imageId)
    {
        // Npgsql retries transient failures, so a user transaction must run
        // inside the provider's execution strategy.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            if (!await db.Courses.AnyAsync(course => course.Id == courseId)) return false;

            var images = await db.CourseImages
                .Where(image => image.CourseId == courseId)
                .ToListAsync();
            if (images.All(image => image.Id != imageId)) return false;

            foreach (var image in images)
                image.IsThumbnail = image.Id == imageId;

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        });
    }

    public async Task<bool> DeleteImageAsync(int courseId, int imageId)
    {
        // Keep deleting the image and promoting its replacement atomic while
        // allowing Npgsql's retry strategy to safely rerun the whole unit.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var image = await db.CourseImages.FirstOrDefaultAsync(item =>
                item.Id == imageId && item.CourseId == courseId);
            if (image is null) return false;

            var wasThumbnail = image.IsThumbnail;
            db.CourseImages.Remove(image);
            await db.SaveChangesAsync();

            if (wasThumbnail)
            {
                var next = await db.CourseImages
                    .Where(item => item.CourseId == courseId)
                    .OrderBy(item => item.Id)
                    .FirstOrDefaultAsync();
                if (next is not null)
                {
                    next.IsThumbnail = true;
                    await db.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return true;
        });
    }
}
