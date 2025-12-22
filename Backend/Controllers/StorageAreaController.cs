using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjArqsi.Application.DTOs.StorageArea;
using ProjArqsi.Application.Services;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.StorageAreaAggregate;

namespace ProjArqsi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ShippingAgentRepresentative, PortAuthorityOfficer, LogisticOperator")]
    public class StorageAreaController : ControllerBase
    {
        private readonly StorageAreaService _service;

        public StorageAreaController(StorageAreaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StorageAreaDto>>> GetAll()
        {
            try
            {
                var areas = await _service.GetAllAsync();
                return Ok(areas);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving storage areas." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StorageAreaDto>> GetById(Guid id)
        {
            try
            {
                var area = await _service.GetByIdAsync(new StorageAreaId(id));
                return Ok(area);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the storage area." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<StorageAreaDto>> Create([FromBody] StorageAreaUpsertDto dto)
        {
            try
            {
                var area = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = area.Id }, area);
            }
            catch (BusinessRuleValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the storage area.", detail = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StorageAreaDto>> Update(Guid id, [FromBody] StorageAreaUpsertDto dto)
        {
            try
            {
                var area = await _service.UpdateAsync(new StorageAreaId(id), dto);
                return Ok(area);
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
                return StatusCode(500, new { message = "An error occurred while updating the storage area." });
            }
        }
    }
}
