using Microsoft.AspNetCore.Mvc;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool? isPinned, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Return mock pinned news if requested
            if (isPinned == true)
            {
                return Ok(new List<object>
                {
                    new
                    {
                        id = 1,
                        title = "Chào mừng đến với CLB Cầu Lông",
                        content = "Thông tin về CLB và các hoạt động sắp tới.",
                        isPinned = true,
                        createdAt = DateTime.Now.AddDays(-7)
                    }
                });
            }

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
            return Ok(new
            {
                id,
                title = "News " + id,
                content = "Content of news " + id,
                isPinned = false,
                createdAt = DateTime.Now
            });
        }

        [HttpPost]
        public IActionResult Create([FromBody] object data)
        {
            return Ok(new { message = "News created (mock)" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object data)
        {
            return Ok(new { message = "News updated (mock)" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }

        [HttpPatch("{id}/pin")]
        public IActionResult TogglePin(int id, [FromBody] object data)
        {
            return Ok(new { message = "Pin status updated (mock)" });
        }
    }
}
