using Microsoft.AspNetCore.Mvc;
using WebApiTemplateWinAuth.Repositories.App;

namespace WebApiTemplateWinAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController(ItemRepository items) : ControllerBase
    {
        [HttpGet("Index")]
        public async Task<IActionResult> IndexAsync(CancellationToken ct)
        {
            return Ok(await items.GetAllAsync(ct));
        }
    }
}
