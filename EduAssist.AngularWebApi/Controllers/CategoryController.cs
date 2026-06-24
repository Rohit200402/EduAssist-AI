using EduAssist.AngularWebApi.DTOs;
using EduAssist.EFCore.Context;
using EduAssist.EFCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

/// <summary>
/// Category Controller - CRUD for academic subjects.
/// GET is available to all authenticated users.
/// POST/PUT/DELETE are Admin only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public CategoryController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// GET /api/category
    /// Gets all categories (for dropdown in Ask Question).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _appDbContext.Categories
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                SubjectName = c.SubjectName,
                TotalQuestions = c.UserRequests.Count
            })
            .OrderBy(c => c.SubjectName)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// GET /api/category/{id}
    /// Gets a specific category by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _appDbContext.Categories
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                SubjectName = c.SubjectName,
                TotalQuestions = c.UserRequests.Count
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound(new { error = "Category not found." });

        return Ok(category);
    }

    /// <summary>
    /// POST /api/category (Admin only)
    /// Creates a new category/subject.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
    {
        // Check for duplicate
        var exists = await _appDbContext.Categories
            .AnyAsync(c => c.SubjectName.ToLower() == categoryDto.SubjectName.ToLower());

        if (exists)
            return BadRequest(new { error = "A category with this name already exists." });

        var category = new Category
        {
            SubjectName = categoryDto.SubjectName
        };

        _appDbContext.Categories.Add(category);
        await _appDbContext.SaveChangesAsync();

        var result = new CategoryDto
        {
            CategoryId = category.CategoryId,
            SubjectName = category.SubjectName,
            TotalQuestions = 0
        };

        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, result);
    }

    /// <summary>
    /// PUT /api/category/{id} (Admin only)
    /// Updates an existing category/subject name.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto categoryDto)
    {
        var category = await _appDbContext.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { error = "Category not found." });

        // Check for duplicate (exclude self)
        var exists = await _appDbContext.Categories
            .AnyAsync(c => c.SubjectName.ToLower() == categoryDto.SubjectName.ToLower()
                           && c.CategoryId != id);

        if (exists)
            return BadRequest(new { error = "A category with this name already exists." });

        category.SubjectName = categoryDto.SubjectName;
        await _appDbContext.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            SubjectName = category.SubjectName,
            TotalQuestions = await _appDbContext.UserRequests.CountAsync(ur => ur.CategoryId == id)
        });
    }

    /// <summary>
    /// DELETE /api/category/{id} (Admin only)
    /// Deletes a category. Fails if questions exist in this category.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _appDbContext.Categories.FindAsync(id);
        if (category == null)
            return NotFound(new { error = "Category not found." });

        // Prevent deletion if questions exist
        var hasQuestions = await _appDbContext.UserRequests
            .AnyAsync(ur => ur.CategoryId == id);

        if (hasQuestions)
            return BadRequest(new { error = "Cannot delete category with existing questions. Reassign or delete questions first." });

        _appDbContext.Categories.Remove(category);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }
}
