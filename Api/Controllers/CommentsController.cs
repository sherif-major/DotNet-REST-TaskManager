using Api.DTOs.Comment;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentsController(CommentService commentService)
    {
        _commentService = commentService;
    }

    
    [Authorize]
    [HttpGet("tasks/{taskId:int}/comments")]
    public async Task<IActionResult> GetByTask(int taskId)
    {
        var comments = await _commentService.GetByTaskIdAsync(taskId);

        var response = comments.Select(c => new CommentResponseDto
        {
            Id = c.Id,
            Content = c.Content,
            TaskItemId = c.TaskItemId,
            UserId = c.UserId
        }).ToList();

        return Ok(ApiResponse<List<CommentResponseDto>>.SuccessResponse(response, "Comments listed"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("tasks/{taskId:int}/comments")]
    public async Task<IActionResult> Create(int taskId, [FromBody] CreateCommentDto dto)
    {
        var result = await _commentService.CreateAsync(taskId, dto.UserId, dto.Content);

        if (!result.ok)
        {
            if (result.error == "Task not found")
                return NotFound(ApiResponse<string>.Fail(result.error));

            return BadRequest(ApiResponse<string>.Fail(result.error!));
        }

        var response = new CommentResponseDto
        {
            Id = result.comment!.Id,
            Content = result.comment.Content,
            TaskItemId = result.comment.TaskItemId,
            UserId = result.comment.UserId
        };

        return Created($"/comments/{response.Id}", ApiResponse<CommentResponseDto>.SuccessResponse(response, "Comment created"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("comments/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto dto)
    {
        var result = await _commentService.UpdateAsync(id, dto.Content);

        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        var response = new CommentResponseDto
        {
            Id = result.comment!.Id,
            Content = result.comment.Content,
            TaskItemId = result.comment.TaskItemId,
            UserId = result.comment.UserId
        };

        return Ok(ApiResponse<CommentResponseDto>.SuccessResponse(response, "Comment updated"));
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("comments/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _commentService.SoftDeleteAsync(id);

        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        return Ok(ApiResponse<string>.SuccessResponse("Comment deleted", "Comment deleted"));
    }
}
