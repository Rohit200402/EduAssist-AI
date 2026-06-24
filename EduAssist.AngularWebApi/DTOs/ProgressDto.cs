namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// DTO for student progress statistics.
/// </summary>
public class ProgressDto
{
    public int TotalQuestionsAsked { get; set; }

    public int QuestionsThisWeek { get; set; }

    public int QuestionsThisMonth { get; set; }

    public int CurrentStreak { get; set; }

    public string? FavoriteSubject { get; set; }

    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
}

/// <summary>
/// DTO for category-wise question breakdown.
/// </summary>
public class CategoryBreakdownDto
{
    public string SubjectName { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}
