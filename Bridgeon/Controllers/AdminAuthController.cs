using Bridgeon.Data;
using Bridgeon.Dtos.RegisterDto;
using Bridgeon.Models;
using Bridgeon.Repositeries.UserRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly AppDbContext _context; // ✅ Add this

    public AdminController(IUserRepository userRepo, AppDbContext context)
    {
        _userRepo = userRepo;
        _context = context; // ✅ assign
    }

    // 1️⃣ Add User
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
            Email = dto.Email.ToLower(),
            FullName = dto.FullName,
            Role = dto.Role ?? "User",
            IsWhitelisted = true
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "User added successfully" });
    }

    // 2️⃣ Get All Users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepo.GetAllAsync();
        var result = users.Select(u => new
        {
            u.Id,
            u.Email,
            u.FullName,
            u.Role,
            u.IsBlocked
        });
        return Ok(result);
    }

    // 3️⃣ Remove User (✅ Fixed)
    [HttpDelete("remove-user/{id}")]
    public async Task<IActionResult> RemoveUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Mentees) // include mentees to handle null assignment
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound("User not found");

        // Set MentorId = null for all mentees before deleting mentor/user
        if (user.Mentees != null && user.Mentees.Any())
        {
            foreach (var mentee in user.Mentees)
            {
                mentee.MentorId = null;
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User removed successfully" });
    }

    // 4️⃣ Block User
    [HttpPatch("block-user/{id}")]
    public async Task<IActionResult> BlockUser(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound("User not found");

        user.IsBlocked = true;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "User blocked successfully" });
    }

    // 5️⃣ Unblock User
    [HttpPatch("unblock-user/{id}")]
    public async Task<IActionResult> UnblockUser(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound("User not found");

        user.IsBlocked = false;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "User unblocked successfully" });
    }

    // 6️⃣ Update User Role
    [HttpPatch("update-role/{id}")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound("User not found");

        if (string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Role is required");

        user.Role = dto.Role;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "User role updated successfully" });
    }
}
