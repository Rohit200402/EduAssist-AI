using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Seed;

public static class CategorySeedData
{
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
