using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Application.DTOs.VVN;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.Shared;
using System.Security.Claims;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselVisitNotificationController : ControllerBase
    {
        private readonly VesselVisitNotificationService _service;

        public VesselVisitNotificationController(VesselVisitNotificationService service)
        {
            _service = service;
        }


        // ----------PORT AUTHORITY OFFICER METHODS----------

        // Get all pending (submitted) VVNs for review
        [HttpGet("pending")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<IEnumerable<VVNDto>>> GetAllPending()
        {
            try
            {
                var vvns = await _service.GetAllPendingAsync();
                return Ok(vvns);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving pending vessel visit notifications.", details = innerMessage });
            }
        }

        // Approve a submitted VVN with temporary dock assignment
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VVNDto>> Approve(Guid id, [FromBody] VVNApprovalDto dto)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("Officer ID not found in token.");

                if (!Guid.TryParse(dto.TempAssignedDockId, out var dockGuid))
                    return BadRequest(new { message = "Invalid dock ID format." });

                var vvn = await _service.ApproveVvnAsync(id, dockGuid, officerId, dto.ConfirmDockConflict);
                return Ok(vvn);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (BusinessRuleValidationException ex)
            {
                // Check if this is a dock conflict error
                if (ex.Message.StartsWith("DOCK_CONFLICT:"))
                {
                    return Conflict(new { message = ex.Message.Replace("DOCK_CONFLICT: ", ""), requiresConfirmation = true });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while approving VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while approving the VVN.", details = innerMessage });
            }
        }

        // Check for dock conflicts before approving
        [HttpGet("{id}/check-dock-conflicts/{dockId}")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<DockConflictInfoDto>> CheckDockConflicts(Guid id, Guid dockId)
        {
            try
            {
                var conflicts = await _service.CheckDockConflictsAsync(id, dockId);
                return Ok(conflicts);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while checking dock conflicts.", details = innerMessage });
            }
        }

        // Reject a submitted VVN with reason
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VVNDto>> Reject(Guid id, [FromBody] VVNRejectionDto dto)
        {
            try
            {
                var officerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("Officer ID not found in token.");

                var vvn = await _service.RejectVvnAsync(id, dto.RejectionReason, officerId);
                return Ok(vvn);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while rejecting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while rejecting the VVN.", details = innerMessage });
            }
        }


        // ----------AGENT SHIP REPRESENTATIVE METHODS----------
        // Get all drafted VVNs
        [HttpGet("drafts")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<IEnumerable<VVNDto>>> GetAllDrafts()
        {
            try
            {
                var vvns = await _service.GetAllDraftsAsync();
                return Ok(vvns);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving drafted vessel visit notifications.", details = innerMessage });
            }
        }

         // Get a drafted VVN by Id
        [HttpGet("drafts/{id}")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> GetDraftById(Guid id)
        {
            try
            {
                var vvn = await _service.GetDraftByIdAsync(id);
                return Ok(vvn);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving the drafted VVN.", details = innerMessage });
            }
        }

        [HttpPost("draft")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> DraftVVN([FromBody] VVNDraftDto dto)
        {
            try
            {
                var vvn = await _service.DraftVVNAsync(dto);
                return CreatedAtAction(nameof(GetDraftById), new { id = vvn.Id }, vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while drafting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while drafting the VVN.", details = innerMessage });
            }
        }

        // Update an existing draft (including manifests)
        [HttpPut("drafts/{id}")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDraftDtoWId>> UpdateDraft(Guid id, [FromBody] VVNDraftDto dto)
        {
            try
            {
                var vvn = await _service.UpdateDraftAsync(id, dto);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while updating draft.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while updating the draft.", details = innerMessage });
            }
        }

        [HttpPost("submit")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> SubmitVVN([FromBody] VVNSubmitDto dto)
        {
            try
            {
                var vvn = await _service.SubmitVVNAsync(dto);
                return CreatedAtAction(nameof(GetSubmittedById), new { id = vvn.Id }, vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while submitting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while submitting the VVN.", details = innerMessage });
            }
        }

        // Submit an existing draft by ID
        [HttpPost("drafts/{id}/submit")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNSubmitDtoWId>> SubmitDraft(Guid id)
        {
            try
            {
                var vvn = await _service.SubmitDraftAsync(id);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while submitting draft.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while submitting the draft.", details = innerMessage });
            }
        }

        // Resubmit a rejected VVN for a new decision
        [HttpPost("{id}/resubmit")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> Resubmit(Guid id)
        {
            try
            {
                var vvn = await _service.ResubmitVvnAsync(id);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while resubmitting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while resubmitting the VVN.", details = innerMessage });
            }
        }

        // Update and resubmit a rejected VVN with new data
        [HttpPut("{id}/resubmit")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> UpdateAndResubmit(Guid id, [FromBody] VVNSubmitDto dto)
        {
            try
            {
                var vvn = await _service.UpdateAndResubmitVvnAsync(id, dto);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while updating and resubmitting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while updating and resubmitting the VVN.", details = innerMessage });
            }
        }

        // Convert rejected VVN back to draft
        [HttpPut("{id}/convert-to-draft")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDraftDtoWId>> ConvertRejectedToDraft(Guid id, [FromBody] VVNDraftDto dto)
        {
            try
            {
                var vvn = await _service.ConvertRejectedToDraftAsync(id, dto);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while converting VVN to draft.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while converting VVN to draft.", details = innerMessage });
            }
        }


         // For Agent Ship Rep: Delete a drafted VVN
        [HttpDelete("drafts/{id}")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<IActionResult> DeleteDraft(Guid id)
        {
            try
            {
                await _service.DeleteDraftAsync(id);
                return NoContent();
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while deleting drafted VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while deleting the drafted VVN.", details = innerMessage });
            }
        }

        // ----------METHODS FOR BOTH----------

        // Get all accepted/rejected VVNs
        [HttpGet("reviewed")]
        [Authorize(Roles = "PortAuthorityOfficer, ShippingAgentRepresentative")]
        public async Task<ActionResult<IEnumerable<VVNDto>>> GetAllReviewed()
        {
            try
            {
                var vvns = await _service.GetAllReviewedAsync();
                return Ok(vvns);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving reviewed vessel visit notifications.", details = innerMessage });
            }
        }

        // Get a single accepted/rejected VVN by Id
        [HttpGet("reviewed/{id}")]
        [Authorize(Roles = "PortAuthorityOfficer, ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> GetReviewedById(Guid id)
        {
            try
            {
                var vvn = await _service.GetReviewedByIdAsync(id);
                return Ok(vvn);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving the reviewed VVN.", details = innerMessage });
            }
        }

        // Get all submitted VVNs
        [HttpGet("submitted")]
        [Authorize(Roles = "PortAuthorityOfficer, ShippingAgentRepresentative")]
        public async Task<ActionResult<IEnumerable<VVNDto>>> GetAllSubmitted()
        {
            try
            {
                var vvns = await _service.GetAllSubmittedAsync();
                return Ok(vvns);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving submitted vessel visit notifications.", details = innerMessage });
            }
        }

        // Get a submitted VVN by Id
        [HttpGet("submitted/{id}")]
        [Authorize(Roles = "PortAuthorityOfficer, ShippingAgentRepresentative")]
        public async Task<ActionResult<VVNDto>> GetSubmittedById(Guid id)
        {
            try
            {
                var vvn = await _service.GetSubmittedByIdAsync(id);
                return Ok(vvn);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving the submitted VVN.", details = innerMessage });
            }
        }

        // Get all approved VVNs for a specific date (for scheduling)
        [HttpGet("approved")]
        [Authorize(Roles = "LogisticOperator, PortAuthorityOfficer")]
        public async Task<ActionResult<IEnumerable<VVNDto>>> GetApprovedForDate([FromQuery] DateTime date)
        {
            try
            {
                Console.WriteLine($"[Backend] GET /api/VesselVisitNotification/approved called with date: {date:yyyy-MM-dd}");
                var vvns = await _service.GetApprovedVVNsForDateAsync(date);
                Console.WriteLine($"[Backend] Service returned {vvns.Count} VVNs");
                return Ok(vvns);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Backend] ERROR in GetApprovedForDate: {ex.Message}");
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while retrieving approved VVNs.", details = innerMessage });
            }
        }

    }
}
