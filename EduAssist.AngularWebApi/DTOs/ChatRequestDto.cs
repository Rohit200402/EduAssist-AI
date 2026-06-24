using System.ComponentModel.DataAnnotations;

namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// Request DTO for asking a question to the AI.
/// </summary>
public class ChatRequestDto
{
    [Required(ErrorMessage = "Question is required.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 2000 characters.")]
    public string Query { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category/Subject selection is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
    public int CategoryId { get; set; }
}
