using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjArqsi.Application.Services;
using ProjArqsi.Application.DTOs;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselController : ControllerBase
    {
        private readonly VesselService _service;
        
        public VesselController(VesselService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<IEnumerable<VesselDto>>> GetAll()
        {
            try
            {
                var vessels = await _service.GetAllAsync();
                return Ok(vessels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        
        [HttpGet("{imo}")]
        [Authorize(Roles = "Admin,PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<VesselDto>> GetByImo(string imo)
        {
            try
            {
                var vessel = await _service.GetByImoAsync(imo);
                return Ok(vessel);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<VesselDto>> CreateVesselAsync([FromBody] CreateVesselDto dto)
        {
            try
            {
                var vessel = await _service.CreateAsync(
                    dto.Imo,
                    dto.VesselName,
                    dto.Capacity,
                    dto.Rows,
                    dto.Bays,
                    dto.Tiers,
                    dto.Length,
                    dto.VesselTypeId
                );

                return CreatedAtAction(nameof(GetByImo), new { imo = vessel.Imo }, vessel);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{imo}")]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<VesselDto>> UpdateVesselAsync(string imo, [FromBody] UpdateVesselDto dto)
        {
            try
            {
                var vessel = await _service.UpdateAsync(
                    imo,
                    dto.VesselName,
                    dto.Capacity,
                    dto.Rows,
                    dto.Bays,
                    dto.Tiers,
                    dto.Length,
                    dto.VesselTypeId
                );

                return Ok(vessel);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{imo}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVesselAsync(string imo)
        {
            try
            {
                var result = await _service.DeleteAsync(imo);

                if (!result)
                {
                    return NotFound(new { message = $"Vessel with IMO '{imo}' not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
