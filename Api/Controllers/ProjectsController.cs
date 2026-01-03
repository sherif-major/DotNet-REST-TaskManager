using Api.DTOs.Project;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectService.GetAllAsync();

        var response = projects.Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            UserId = p.UserId
        }).ToList();

        return Ok(ApiResponse<List<ProjectResponseDto>>.SuccessResponse(response, "Projects listed"));
    }

    
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project is null)
            return NotFound(ApiResponse<string>.Fail("Project not found"));

        var response = new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            UserId = project.UserId
        };

        return Ok(ApiResponse<ProjectResponseDto>.SuccessResponse(response, "Project retrieved"));
    }

    
    [Authorize]
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var projects = await _projectService.GetByUserIdAsync(userId);

        var response = projects.Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            UserId = p.UserId
        }).ToList();

        return Ok(ApiResponse<List<ProjectResponseDto>>.SuccessResponse(response, "User projects listed"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await _projectService.CreateAsync(dto.Name, dto.Description, dto.UserId);

        if (!result.ok)
            return BadRequest(ApiResponse<string>.Fail(result.error!));

        var response = new ProjectResponseDto
        {
            Id = result.project!.Id,
            Name = result.project.Name,
            Description = result.project.Description,
            UserId = result.project.UserId
        };

        return Created($"/projects/{response.Id}", ApiResponse<ProjectResponseDto>.SuccessResponse(response, "Project created"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        var result = await _projectService.UpdateAsync(id, dto.Name, dto.Description);

        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        var response = new ProjectResponseDto
        {
            Id = result.project!.Id,
            Name = result.project.Name,
            Description = result.project.Description,
            UserId = result.project.UserId
        };

        return Ok(ApiResponse<ProjectResponseDto>.SuccessResponse(response, "Project updated"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _projectService.SoftDeleteAsync(id);

        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        return Ok(ApiResponse<string>.SuccessResponse("Project deleted", "Project deleted"));
    }
}
