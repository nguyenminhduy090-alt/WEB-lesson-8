using CourseManager.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManager.Web.Data;

public class CourseDbContext(DbContextOptions<CourseDbContext> options) : DbContext(options)
{
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseImage> CourseImages => Set<CourseImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseCategory>()
            .HasIndex(category => category.Name).IsUnique();

        modelBuilder.Entity<Course>()
            .HasIndex(course => new { course.Name, course.CourseCategoryId })
            .IsUnique()
            .HasDatabaseName("uk_course_name_category");

        modelBuilder.Entity<Course>()
            .HasOne(course => course.CourseCategory)
            .WithMany(category => category.Courses)
            .HasForeignKey(course => course.CourseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasMany(course => course.Images)
            .WithOne(image => image.Course)
            .HasForeignKey(image => image.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
