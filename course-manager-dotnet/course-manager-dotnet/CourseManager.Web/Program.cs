using CourseManager.Web.Data;
using CourseManager.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CourseDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
});
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

// Luôn dùng trang lỗi thân thiện, kể cả khi đang chạy để demo.
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Courses}/{action=Index}/{id?}");

// Tạo bảng và dữ liệu demo nếu database vừa được tạo.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DemoData.SeedAsync(db);
}

app.Run();
