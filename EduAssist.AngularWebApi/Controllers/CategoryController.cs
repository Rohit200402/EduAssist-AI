using System.Security.Claims;
using EduAssist.AngularWebApi.Context;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

[ApiController]
[Route("api/category")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                SubjectName = c.SubjectName,
                TotalQuestions = c.UserRequests.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                SubjectName = c.SubjectName,
                TotalQuestions = c.UserRequests.Count
            })
            .FirstOrDefaultAsync();

        if (category == null) return NotFound(new { error = "Category not found." });

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.SubjectName.ToLower() == dto.SubjectName.ToLower());

        if (exists) return BadRequest(new { error = "A category with this name already exists." });

        var category = new Category
        {
            SubjectName = dto.SubjectName
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            SubjectName = category.SubjectName,
            TotalQuestions = 0
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(new { error = "Category not found." });

        var duplicate = await _context.Categories
            .AnyAsync(c => c.SubjectName.ToLower() == dto.SubjectName.ToLower() && c.CategoryId != id);

        if (duplicate) return BadRequest(new { error = "A category with this name already exists." });

        category.SubjectName = dto.SubjectName;
        await _context.SaveChangesAsync();

        var totalQuestions = await _context.UserRequests.CountAsync(ur => ur.CategoryId == id);

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            SubjectName = category.SubjectName,
            TotalQuestions = totalQuestions
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound(new { error = "Category not found." });

        var hasQuestions = await _context.UserRequests.AnyAsync(ur => ur.CategoryId == id);
        if (hasQuestions) return BadRequest(new { error = "Cannot delete category with existing questions." });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Category deleted successfully." });
    }
}
