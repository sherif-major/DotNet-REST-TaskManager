using Api.DTOs.User;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();

        var response = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Role = u.Role
        }).ToList();

        return Ok(ApiResponse<List<UserResponseDto>>.SuccessResponse(response, "Users listed"));
    }

    
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<string>.Fail("User not found"));

        var response = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };

        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(response, "User retrieved"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto.Username, dto.Password, "User");

        if (!result.ok)
            return BadRequest(ApiResponse<string>.Fail(result.error!));

        var response = new UserResponseDto
        {
            Id = result.user!.Id,
            Username = result.user.Username,
            Role = result.user.Role
        };

        return Created($"/users/{response.Id}", ApiResponse<UserResponseDto>.SuccessResponse(response, "User created"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserDto dto)
    {
        var allowed = new[] { "Admin", "User" };
        if (!allowed.Contains(dto.Role))
            return BadRequest(ApiResponse<string>.Fail("Invalid role. Use: Admin, User"));

        var result = await _userService.UpdateRoleAsync(id, dto.Role);
        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        return Ok(ApiResponse<string>.SuccessResponse("Role updated", "Role updated"));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.SoftDeleteAsync(id);
        if (!result.ok)
            return NotFound(ApiResponse<string>.Fail(result.error!));

        return Ok(ApiResponse<string>.SuccessResponse("User deleted", "User deleted"));
    }
}
