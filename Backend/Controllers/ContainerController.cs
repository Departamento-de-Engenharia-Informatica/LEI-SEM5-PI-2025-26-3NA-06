using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjArqsi.Application.Services;
using ProjArqsi.Application.DTOs;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContainerController : ControllerBase
    {
        private readonly ContainerService _service;

        public ContainerController(ContainerService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<IEnumerable<ContainerDto>>> GetAll()
        {
            try
            {
                var containers = await _service.GetAllAsync();
                return Ok(containers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<ContainerDto>> GetById(Guid id)
        {
            try
            {
                var container = await _service.GetByIdAsync(id);
                return Ok(container);
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

        [HttpGet("iso/{isoCode}")]
        [Authorize(Roles = "PortAuthorityOfficer,ShippingAgentRepresentative")]
        public async Task<ActionResult<ContainerDto>> GetByIsoCode(string isoCode)
        {
            try
            {
                var container = await _service.GetByIsoCodeAsync(isoCode);
                return Ok(container);
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

        [HttpPost]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<ContainerDto>> CreateContainer([FromBody] UpsertContainerDto dto)
        {
            try
            {
                var container = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = container.Id }, container);
            }
            catch (BusinessRuleValidationException ex)
            {
                // Check if it's a duplicate ISO code (409 Conflict)
                if (ex.Message.Contains("already exists") || ex.Message.Contains("unique"))
                {
                    return Conflict(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult<ContainerDto>> UpdateContainer(Guid id, [FromBody] UpsertContainerDto dto)
        {
            try
            {
                var container = await _service.UpdateAsync(id, dto);
                return Ok(container);
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
        [Authorize(Roles = "ShippingAgentRepresentative")]
        public async Task<ActionResult> DeleteContainer(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
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
    }
}
