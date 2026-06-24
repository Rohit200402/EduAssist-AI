using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.EFCore.Seed;

/// <summary>
/// Seeds the Categories table with default academic subjects.
/// These are applied during database migration so the application
/// has initial subjects available for students to categorize their questions.
/// </summary>
public static class CategorySeedData
{
    /// <summary>
    /// Applies seed data for the Category entity to the model builder.
    /// Call this from AppDbContext.OnModelCreating or use as a separate migration seed.
    /// </summary>
    public static void SeedCategories(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, SubjectName = "Mathematics" },
            new Category { CategoryId = 2, SubjectName = "Physics" },
            new Category { CategoryId = 3, SubjectName = "Chemistry" },
            new Category { CategoryId = 4, SubjectName = "Biology" },
            new Category { CategoryId = 5, SubjectName = "English" },
            new Category { CategoryId = 6, SubjectName = "Computer Science" },
            new Category { CategoryId = 7, SubjectName = "History" },
            new Category { CategoryId = 8, SubjectName = "Geography" },
            new Category { CategoryId = 9, SubjectName = "Economics" },
            new Category { CategoryId = 10, SubjectName = "Political Science" }
        );
    }
}
