using Microsoft.AspNetCore.Mvc;
using WebApiTemplateWinAuth.Models;
using WebApiTemplateWinAuth.Services;

namespace WebApiTemplateWinAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController(UserDirectoryService directory) : ControllerBase
    {
        /// <summary>
        /// Returns the Windows user of the current request, e.g. { "name": "DOMAIN\\username", "displayName": "Jon Doe" }.
        /// The browser signs in automatically; a frontend on another origin must call it with credentials: 'include'.
        /// </summary>
        [HttpGet("Login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<UserInfo> Login()
        {
            var name = User.Identity?.Name;
            if (string.IsNullOrEmpty(name))
                return Challenge();

            return new UserInfo
            {
                Name = name,
                DisplayName = directory.GetDisplayName(name)
            };
        }
    }
}
