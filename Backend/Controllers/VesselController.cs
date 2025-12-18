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
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
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

        [HttpGet("{id}")]
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<VesselDto>> GetById(Guid id)
        {
            try
            {
                var vessel = await _service.GetByIdAsync(id);
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

        [HttpGet("imo/{imo}")]
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
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
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VesselDto>> CreateVesselAsync([FromBody] UpsertVesselDto dto)
        {
            try
            {
                var vessel = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = vessel.Id }, vessel);}
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

        [HttpPut("{id}")]
        [Authorize(Roles = "PortAuthorityOfficer")]
        public async Task<ActionResult<VesselDto>> UpdateVesselAsync(Guid id, [FromBody] UpsertVesselDto dto)
        {
            try
            {
                var vessel = await _service.UpdateAsync(id, dto);
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteVesselAsync(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Vessel with ID '{id}' not found." });
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
