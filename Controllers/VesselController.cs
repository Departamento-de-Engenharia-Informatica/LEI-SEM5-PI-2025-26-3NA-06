using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.VesselAggregate;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselController : ControllerBase
    {
        private readonly VesselService service;
        
        public VesselController( VesselService service)
        {
            this.service = service ;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vessel>>> GetAll()
        {
            return Ok(await service.GetAllAsync());
        }

        
        [HttpGet("{imo}")]
        public ActionResult<object> GetByImo(string imo)
        {
            // TODO: Implement repository call
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Vessel>> CreateVesselAsync([FromBody] object vesselDto)
        {
            try
            {
                // TODO: Convert vesselDto to proper DTO parameters
                // TODO: Validate VesselType exists and is active
                // TODO: Validate Owner (ShippingAgentOrg) exists and is active
                
                var vessel = await service.CreateAsync(
                    "IMO1234567",  // TODO: Get from DTO
                    "VesselName",  // TODO: Get from DTO
                    1000,          // TODO: Get from DTO
                    100.0,         // TODO: Get from DTO
                    500,           // TODO: Get from DTO
                    Guid.Empty,    // TODO: Get ownerId from DTO
                    Guid.Empty     // TODO: Get vesselTypeId from DTO
                );

                return CreatedAtAction(nameof(GetByImo), new { imo = vessel.Id.AsString() }, vessel);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{imo}")]
        public async Task<ActionResult<Vessel>> UpdateVesselAsync(string imo, [FromBody] object vesselDto)
        {
            try
            {
                // TODO: Convert vesselDto to proper parameters
                var vessel = await service.UpdateAsync(imo, "TempName", 1000, 100.0, 500, Guid.Empty);

                if (vessel == null)
                {
                    return NotFound();
                }

                return Ok(vessel);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{imo}")]
        public async Task<ActionResult<Vessel>> DeleteVesselAsync(string imo)
        {
            try
            {
                var result = await service.DeleteAsync(imo);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
