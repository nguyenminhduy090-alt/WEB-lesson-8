using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Web.Models;

[Table("course_images")]
public class CourseImage
{
    public int Id { get; set; }

    [Required]
    [StringLength(1000)]
    public string ImageUrl { get; set; } = "";

    public bool IsThumbnail { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}
