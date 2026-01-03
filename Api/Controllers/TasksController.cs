using Api.DTOs.Task;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    
    [Authorize]
    [HttpGet("projects/{projectId:int}/tasks")]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        var tasks = await _taskService.GetByProjectIdAsync(projectId);

        var response = tasks.Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            ProjectId = t.ProjectId
        }).ToList();

        return Ok(ApiResponse<List<TaskResponseDto>>.SuccessResponse(response, "Tasks listed"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("projects/{projectId:int}/tasks")]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.CreateAsync(projectId, dto.Title, dto.Description);

        if (!result.ok)
            return BadRequest(ApiResponse<string>.Fail(result.error!));

        var response = new TaskResponseDto
        {
            Id = result.task!.Id,
            Title = result.task.Title,
            Description = result.task.Description,
            Status = result.task.Status,
            ProjectId = result.task.ProjectId
        };

        return Created($"/tasks/{response.Id}", ApiResponse<TaskResponseDto>.SuccessResponse(response, "Task created"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("tasks/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var result = await _taskService.UpdateAsync(id, dto.Title, dto.Description, dto.Status);

        if (!result.ok)
        {
            // status invalid ise 400, task yoksa 404 yapmak istersen söyle, şimdilik tek tip veriyoruz
            if (result.error == "Task not found")
                return NotFound(ApiResponse<string>.Fail(result.error));

            return BadRequest(ApiResponse<string>.Fail(result.error!));
        }

        var response = new TaskResponseDto
        {
            Id = result.task!.Id,
            Title = result.task.Title,
            Description = result.task.Description,
            Status = result.task.Status,
            ProjectId = result.task.ProjectId
        };

        return Ok(ApiResponse<TaskResponseDto>.SuccessResponse(response, "Task updated"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _taskService.SoftDeleteAsync(id);

        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        return Ok(ApiResponse<string>.SuccessResponse("Task deleted", "Task deleted"));
    }
}
