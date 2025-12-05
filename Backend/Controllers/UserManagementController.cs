using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Services;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using ProjArqsi.DTOs.User;
using ProjArqsi.Application.DTOs.User;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserService _userService;

        public UserManagementController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("inactive-users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetInactiveUsers()
        {
            var inactiveUsers = await _userService.GetInactiveUsersAsync();
            return Ok(inactiveUsers);
        }

        [HttpGet("all-users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpPut("{id}/assign-role")]
        public async Task<ActionResult> AssignRoleAndActivate(Guid id, [FromBody] AssignRoleDto dto)
        {
            try
            {
                var roleType = Enum.Parse<RoleType>(dto.Role);
                var result = await _userService.AssignRoleAndSendActivationEmailAsync(id, roleType);
                
                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "Role assigned and activation email sent" });
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
                
                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var message = result.IsActive ? "User activated" : "User deactivated";
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

