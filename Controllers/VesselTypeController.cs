using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<VesselTypeDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Vessel type with id '{id}' not found." });
            
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VesselTypeDto>>> GetAll()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VesselTypeDto>>> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Search term 'name' is required." });

            var results = await _service.SearchByNameAsync(name);
            return Ok(results);
        }
    }
}
