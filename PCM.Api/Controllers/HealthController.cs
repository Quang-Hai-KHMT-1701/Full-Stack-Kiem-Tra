using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Data;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Test database connection
                await _context.Database.CanConnectAsync();
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    database = "connected"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    database = "disconnected",
                    error = ex.Message
                });
            }
        }
    }
}
