using CourseManager.Web.Services;
using CourseManager.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CourseManager.Web.Controllers;

public class CategoriesController(CategoryService categories) : Controller
{
    public async Task<IActionResult> Index() => View(new CategoryListViewModel { Categories = await categories.ListAsync() });

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel form)
    {
        if (!ModelState.IsValid) return View(form);
        var error = await categories.SaveAsync(form);
        if (error is not null) { ModelState.AddModelError(string.Empty, error); return View(form); }
        TempData["Success"] = "Đã thêm loại khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await categories.FindAsync(id);
        return category is null ? NotFound() : View(new CategoryFormViewModel
        {
            Id = category.Id, Name = category.Name, Description = category.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel form)
    {
        if (id != form.Id) return BadRequest();
        if (!ModelState.IsValid) return View(form);
        try
        {
            var error = await categories.SaveAsync(form);
            if (error is not null) { ModelState.AddModelError(string.Empty, error); return View(form); }
        }
        catch (KeyNotFoundException) { return NotFound(); }
        TempData["Success"] = "Đã cập nhật loại khóa học.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var error = await categories.DeleteAsync(id);
            TempData[error is null ? "Success" : "Error"] = error ?? "Đã xóa loại khóa học.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }
}
