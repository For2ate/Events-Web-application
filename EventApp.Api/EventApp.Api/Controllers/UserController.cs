using EventApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase{

        private readonly IUserService _userService;

        public UserController(IUserService userService) {
         
            _userService = userService;

        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId) {

            var user = await _userService.GetUserByIdAsync(userId);

            return Ok(user);

        }

    }

}

