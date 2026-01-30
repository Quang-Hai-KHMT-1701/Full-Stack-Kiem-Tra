using Microsoft.AspNetCore.Mvc;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Return empty list for now
            return Ok(new
            {
                data = new List<object>(),
                total = 0,
                page,
                pageSize
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return NotFound();
        }

        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            // Return mock summary data
            return Ok(new
            {
                income = 0,
                expense = 0,
                balance = 0
            });
        }

        [HttpGet("by-category")]
        public IActionResult GetByCategory()
        {
            return Ok(new List<object>());
        }

        [HttpPost]
        public IActionResult Create([FromBody] object data)
        {
            return Ok(new { message = "Transaction created (mock)" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object data)
        {
            return Ok(new { message = "Transaction updated (mock)" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }

        [HttpGet("export")]
        public IActionResult Export()
        {
            return File(new byte[0], "application/octet-stream", "transactions.xlsx");
        }
    }
}
