using EventApp.Api.Core.Interfaces;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[Controller]")]
    public class AuthController : ControllerBase {

        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        
        public AuthController (IAuthService authService, ITokenService tokenService, IUserService userService) {

            _authService = authService;
            _tokenService = tokenService;
            _userService = userService;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel model) {

            var registeredUser = await _authService.Register(model);

            var tokens = _tokenService.GenerateTokens(registeredUser);

            return Ok(new { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestModel model) {

            try {

                var loginedUser = await _authService.Login(model);

                var tokens = _tokenService.GenerateTokens(loginedUser);

                return Ok(new { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken });

            } catch (Exception ex) {

                throw;

            }

        }

        [HttpGet("refresh/{refreshToken}")]
        public async Task<IActionResult> Refresh(string refreshToken) {

            try {

                var principal = _tokenService.ValidateRefreshToken(refreshToken);
                var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));

                if (userId == null) {
                    return Unauthorized("Invalid refresh token: User ID not found.");
                }

                var user = await _userService.GetUserByIdAsync(userId);


                var newToken = _tokenService.GenerateTokens(user);

                return Ok(new { AccessToken = newToken.AccessToken });

            } catch (Exception ex) {

                throw;

            }

        }
        


    }

}
