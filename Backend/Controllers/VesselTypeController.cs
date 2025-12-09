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
        public async Task<ActionResult<VesselTypeDto>> Create([FromBody] VesselTypeUpsertDto dto)
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
        public async Task<ActionResult<VesselTypeDto>> Update(Guid id, [FromBody] VesselTypeUpsertDto dto)
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
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
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
        public async Task<ActionResult<IEnumerable<VesselTypeDto>>> Search([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term is required." });
            }

            var results = await _service.SearchByNameOrDescriptionAsync(searchTerm);
            return Ok(results);
        }
    }
}
