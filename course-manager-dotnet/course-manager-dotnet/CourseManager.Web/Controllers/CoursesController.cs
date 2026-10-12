using CourseManager.Web.Models;
using CourseManager.Web.Services;
using CourseManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseManager.Web.Controllers;

public class CoursesController(CourseService courses, CategoryService categories) : Controller
{
    public async Task<IActionResult> Index([FromQuery] string? search, int? categoryId, bool? active,
        decimal? minFee, decimal? maxFee, string sort = "", int page = 0)
    {
        var filters = new CourseListViewModel
        {
            Search = search, CategoryId = categoryId, Active = active,
            MinFee = minFee, MaxFee = maxFee, Sort = sort, Page = page
        };
        if (minFee.HasValue && maxFee.HasValue && minFee > maxFee)
        {
            filters.ErrorMessage = "Học phí tối thiểu không được lớn hơn học phí tối đa.";
            filters.Categories = await categories.ChoicesAsync();
            return View(filters);
        }
        return View(await courses.SearchAsync(filters));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoryChoicesAsync();
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoryChoicesAsync(form.CourseCategoryId);
            return View(form);
        }
        var error = await courses.SaveAsync(form);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            await LoadCategoryChoicesAsync(form.CourseCategoryId);
            return View(form);
        }
        TempData["Success"] = "Đã thêm khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await courses.FindAsync(id);
        if (course is null) return NotFound();
        await LoadCategoryChoicesAsync(course.CourseCategoryId);
        return View(new CourseFormViewModel
        {
            Id = course.Id, Name = course.Name, TuitionFee = course.TuitionFee,
            Duration = course.Duration, IsActive = course.IsActive,
            Description = course.Description, CourseCategoryId = course.CourseCategoryId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel form)
    {
        if (form.Id != id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadCategoryChoicesAsync(form.CourseCategoryId);
            return View(form);
        }
        try
        {
            var error = await courses.SaveAsync(form);
            if (error is not null)
            {
                ModelState.AddModelError(string.Empty, error);
                await LoadCategoryChoicesAsync(form.CourseCategoryId);
                return View(form);
            }
        }
        catch (KeyNotFoundException) { return NotFound(); }
        TempData["Success"] = "Đã cập nhật khóa học.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await courses.FindAsync(id);
        return course is null ? NotFound() : View(new CourseDetailViewModel { Course = course });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int id, CourseImageFormViewModel form)
    {
        var course = await courses.FindAsync(id);
        if (course is null) return NotFound();
        if (!ModelState.IsValid)
            return View("Details", new CourseDetailViewModel { Course = course, ImageUrl = form.ImageUrl });
        await courses.AddImageAsync(id, form.ImageUrl);
        TempData["Success"] = "Đã thêm ảnh vào khóa học.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChooseThumbnail(int id, int imageId)
    {
        if (!await courses.ChooseThumbnailAsync(id, imageId)) return NotFound();
        TempData["Success"] = "Đã đổi ảnh đại diện.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        if (!await courses.DeleteImageAsync(id, imageId)) return NotFound();
        TempData["Success"] = "Đã xóa ảnh.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try { await courses.DeleteAsync(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        TempData["Success"] = "Đã xóa khóa học.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoryChoicesAsync(int? selectedId = null)
    {
        var rows = await categories.ChoicesAsync();
        ViewBag.Categories = new SelectList(rows, "Id", "Name", selectedId);
    }
}
