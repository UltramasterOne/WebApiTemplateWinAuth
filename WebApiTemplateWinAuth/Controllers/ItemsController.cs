using Microsoft.AspNetCore.Mvc;
using WebApiTemplateWinAuth.Models;
using WebApiTemplateWinAuth.Repositories.App;

namespace WebApiTemplateWinAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController(ItemRepository items) : ControllerBase
    {
        [HttpGet("Index")]
        public async Task<ActionResult<List<Item>>> IndexAsync(CancellationToken ct)
        {
            return await items.GetAllAsync(ct);
        }
    }
}
