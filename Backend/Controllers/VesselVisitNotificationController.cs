using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjArqsi.Application.DTOs.VVN;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.Shared;

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

        // Accept a submitted VVN
        [HttpPost("{id}/accept")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VVNDto>> Accept(Guid id)
        {
            try
            {
                var vvn = await _service.AcceptAsync(id);
                return Ok(vvn);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Database error while accepting VVN.", details = innerMessage });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "An error occurred while accepting the VVN.", details = innerMessage });
            }
        }

        // Reject a submitted VVN
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VVNDto>> Reject(Guid id, [FromBody] VVNRejectDto dto)
        {
            try
            {
                var vvn = await _service.RejectAsync(id, dto.RejectionReason);
                return Ok(vvn);
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

    }
}
