using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.DTOs.Dock;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.DockAggregate;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "PortAuthorityOfficer")]
    public class DockController : ControllerBase
    {
        private readonly DockService _service;

        public DockController(DockService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DockDto>>> GetAll()
        {
            try
            {
                var docks = await _service.GetAllAsync();
                return Ok(docks);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving docks." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DockDto>> GetById(Guid id)
        {
            try
            {
                var dock = await _service.GetByIdAsync(new DockId(id));
                return Ok(dock);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the dock." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DockDto>> Create([FromBody] DockUpsertDto dto)
        {
            try
            {
                var dock = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = dock.Id }, dock);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while creating the dock." });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DockDto>> Update(Guid id, [FromBody] DockUpsertDto dto)
        {
            try
            {
                var dock = await _service.UpdateAsync(new DockId(id), dto);
                return Ok(dock);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating the dock." });
            }
        }
    }
}
