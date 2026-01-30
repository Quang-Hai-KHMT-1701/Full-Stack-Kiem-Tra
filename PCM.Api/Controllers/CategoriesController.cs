using Microsoft.AspNetCore.Mvc;

namespace PCM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        // Categories cố định cho hệ thống tài chính CLB
        private static readonly List<object> _categories = new List<object>
        {
            new { id = 1, name = "Thu phí thành viên", type = "income", description = "Phí đăng ký và gia hạn thành viên CLB" },
            new { id = 2, name = "Thu phí sân", type = "income", description = "Phí thuê sân theo giờ" },
            new { id = 3, name = "Thu phí giải đấu", type = "income", description = "Phí đăng ký tham gia giải đấu" },
            new { id = 4, name = "Tài trợ", type = "income", description = "Tiền tài trợ từ các đơn vị" },
            new { id = 5, name = "Chi phí bảo trì", type = "expense", description = "Chi phí sửa chữa, bảo dưỡng sân" },
            new { id = 6, name = "Chi phí vận hành", type = "expense", description = "Điện, nước, nhân công" },
            new { id = 7, name = "Chi phí giải đấu", type = "expense", description = "Giải thưởng, cúp, huy chương" },
            new { id = 8, name = "Chi phí khác", type = "expense", description = "Các chi phí phát sinh khác" }
        };

        // =========================
        // GET: api/categories
        // =========================
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_categories);
        }

        // =========================
        // GET: api/categories/{id}
        // =========================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var category = _categories.FirstOrDefault(c => ((dynamic)c).id == id);
            if (category == null)
                return NotFound(new { message = $"Category với id {id} không tồn tại" });

            return Ok(category);
        }

        // =========================
        // GET: api/categories/by-type/{type}
        // =========================
        [HttpGet("by-type/{type}")]
        public IActionResult GetByType(string type)
        {
            var filtered = _categories
                .Where(c => ((dynamic)c).type == type.ToLower())
                .ToList();

            return Ok(filtered);
        }

        [HttpPost]
        public IActionResult Create([FromBody] object data)
        {
            return BadRequest(new { message = "Categories là danh sách cố định, không thể thêm mới" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object data)
        {
            return BadRequest(new { message = "Categories là danh sách cố định, không thể cập nhật" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return BadRequest(new { message = "Categories là danh sách cố định, không thể xóa" });
        }
    }
}
