using EventApp.Api.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase{

        private readonly IUserService _userService;

        public UserController(IUserService userService) {
         
            _userService = userService;

        }

        [Authorize]
        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetUserById(Guid userId) {

            var user = await _userService.GetUserByIdAsync(userId);

            return Ok(user);

        }

    }

}

