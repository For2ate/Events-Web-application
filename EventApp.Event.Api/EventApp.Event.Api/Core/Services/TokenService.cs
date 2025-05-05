using EventApp.Api.Core.Interfaces;
using EventApp.Data.Entities;
using EventApp.Models.SharedDTO;
using EventApp.Models.UserDTO.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventApp.Api.Core.Services {

    public class TokenService : ITokenService {

        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly double _accessTokenLifetimeMinutes;
        private readonly double _refreshTokenLifetimeDays;

        public TokenService(IConfiguration configuration) {

            _configuration = configuration;
            var jwtSettings = _configuration.GetSection("Jwt");

            var secretKey = jwtSettings["Key"];
           
            if (!double.TryParse(jwtSettings["AccessTokenLifetimeMinutes"], out _accessTokenLifetimeMinutes)) {
                _accessTokenLifetimeMinutes = 15; 
            }
            if (!double.TryParse(jwtSettings["RefreshTokenLifetimeDays"], out _refreshTokenLifetimeDays)) {
                _refreshTokenLifetimeDays = 7;
            }

            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        }

        public TokensResponse GenerateTokens(UserFullResponseModel user) {

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user); 

            return new TokensResponse(accessToken, refreshToken);

        }

        private string GenerateAccessToken(UserFullResponseModel user) {

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string GenerateRefreshToken(UserFullResponseModel user) {

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("userId", user.Id.ToString()),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_refreshTokenLifetimeDays), 
                signingCredentials: credentials);
           

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateRefreshToken(string token) {

            var tokenHandler = new JwtSecurityTokenHandler();

            try {

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey,
                    ValidateLifetime = true, 
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);


                return principal;

            } catch (Exception ex) {

                throw new SecurityTokenException("Invalid refresh token", ex);

            }
        }

    }

}
