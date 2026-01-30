using Microsoft.AspNetCore.Mvc;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            // Return mock categories
            return Ok(new List<object>
            {
                new { id = 1, name = "Thu phí thành viên", type = "income" },
                new { id = 2, name = "Thu phí sân", type = "income" },
                new { id = 3, name = "Chi phí bảo trì", type = "expense" },
                new { id = 4, name = "Chi phí vận hành", type = "expense" }
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok(new { id, name = "Category " + id, type = "income" });
        }

        [HttpPost]
        public IActionResult Create([FromBody] object data)
        {
            return Ok(new { message = "Category created (mock)" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object data)
        {
            return Ok(new { message = "Category updated (mock)" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent();
        }
    }
}
