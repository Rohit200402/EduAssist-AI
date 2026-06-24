using System.ComponentModel.DataAnnotations;

namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// DTO for category/subject display and creation.
/// </summary>
public class CategoryDto
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Subject name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Subject name must be between 2 and 150 characters.")]
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>
    /// Total questions asked in this category (read-only, for admin display).
    /// </summary>
    public int TotalQuestions { get; set; }
}
