using LiquidLabs.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiquidLabsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemUnitController : ControllerBase
    {
        private readonly ISystemUnitService _service;

        public SystemUnitController(ISystemUnitService service)
        {
            _service = service;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> Fetch()
        {
            try
            {
                var result = await _service.GetAllSystemUnitsAsync();
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var unit = await _service.GetSystemUnitByIdAsync(id);
                if (unit == null)
                {
                    return NotFound(new { error = $"System unit with Id={id} not found" });
                }

                return Ok(unit);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Failed to retrieve the record" });
            }
        }
    }
}
