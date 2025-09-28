using Bridgeon.Models;
using Bridgeon.Repositeries.UserRepo;
using Microsoft.AspNetCore.Mvc;
using Bridgeon.Dtos;
using Bridgeon.Dtos.RegisterDto;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public AdminController(IUserRepository userRepo) => _userRepo = userRepo;

  
    [HttpPost("add-user")]
    public async Task<IActionResult> AddUser([FromBody] AddUserRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data");

        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing != null)
            return BadRequest("Email already exists");

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,  // Admin sets full name
            Role = dto.Role ?? "User", // Admin sets role
            IsWhitelisted = true
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "User added successfully" });
    }

}
