using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Services;
using ProjArqsi.DTOs.User;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("inactive-users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetInactiveUsers()
        {
            try
            {
                var inactiveUsers = await _userService.GetInactiveUsersAsync();
                return Ok(inactiveUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all-users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/assign-role")]
        public async Task<ActionResult> AssignRoleAndActivate(Guid id, [FromBody] string role)
        {
            try
            {
                var roleType = Enum.Parse<RoleType>(role);
                await _userService.AssignRoleAndSendActivationEmailAsync(id, roleType);
                return Ok(new { message = "Role assigned and activation email sent" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult> ToggleUserActive(Guid id)
        {
            try
            {
                var result = await _userService.ToggleUserActiveAsync(id);
                var message = result.IsActive ? "User activated" : "User deactivated";
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserUpsertDto dto)
        {
            try
            {
                await _userService.SelfRegisterUserAsync(dto, dto.Email);

                return Ok(new 
                { 
                    success = true,
                    message = "Registration successful! Please wait for an administrator to approve your account and assign you a role."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            try
            {
                await _userService.ConfirmEmailAsync(token);
                return Ok(new { success = true, message = "Email confirmed successfully! Your account is now active." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

