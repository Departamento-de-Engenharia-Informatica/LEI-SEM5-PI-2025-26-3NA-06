using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjArqsi.Application.DTOs;
using ProjArqsi.Application.Services;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselTypeController : ControllerBase
    {
        private readonly VesselTypeService _service;

        public VesselTypeController(VesselTypeService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<VesselTypeDto>> Create([FromBody] CreateVesselTypeDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<VesselTypeDto>> Update(Guid id, [FromBody] UpdateVesselTypeDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<VesselTypeDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Vessel type with id '{id}' not found." });
            
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<IEnumerable<VesselTypeDto>>> GetAll()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,PortAuthorityOfficer")]
        public async Task<ActionResult<IEnumerable<VesselTypeDto>>> Search(
            [FromQuery] string? name = null,
            [FromQuery] string? description = null,
            [FromQuery] string? searchTerm = null)
        {
            // Support searching by name, description, or both (searchTerm searches both fields)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var results = await _service.SearchByNameOrDescriptionAsync(searchTerm);
                return Ok(results);
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                var results = await _service.SearchByNameAsync(name);
                return Ok(results);
            }
            else if (!string.IsNullOrWhiteSpace(description))
            {
                var results = await _service.SearchByDescriptionAsync(description);
                return Ok(results);
            }
            else
            {
                return BadRequest(new { message = "At least one search parameter (name, description, or searchTerm) is required." });
            }
        }
    }
}
