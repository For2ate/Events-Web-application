using EventApp.Api.Core.Interfaces;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase {

        private readonly IAuthService _authService;
        
        public AuthController (IAuthService authService) {

            _authService = authService;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel model) {

            var registeredUser = await _authService.Register(model);

            return Ok(registeredUser);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel model) {

            var loginedUser = await _authService.Login(model);

            return Ok(loginedUser);

        }
    
    }

}
