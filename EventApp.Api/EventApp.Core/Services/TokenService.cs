using EventApp.Core.Interfaces;
using EventApp.Models.SharedDTO;
using EventApp.Models.UserDTO.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventApp.Core.Services {

    public class TokenService : ITokenService {

        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _securityKey;
        private readonly string _issuer; 
        private readonly string _audience;
        private readonly double _accessTokenLifetimeMinutes;
        private readonly double _refreshTokenLifetimeDays;

        public TokenService(IConfiguration configuration) {

            _configuration = configuration;
            var jwtSettings = _configuration.GetSection("Jwt");

            var secretKey = jwtSettings["Key"];
            _issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer"); 
            _audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Jwt:Audience"); 

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
            var refreshToken = GenerateRefreshToken(user.Id); 

            return new TokensResponse(accessToken, refreshToken);

        }

        private string GenerateAccessToken(UserFullResponseModel user) {

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FirstName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,      
                audience: _audience,   
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string GenerateRefreshToken(Guid userId) {

            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                 issuer: _issuer,     
                 audience: _audience,
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

                    ValidateIssuer = true,         
                    ValidIssuer = _issuer,        

                    ValidateAudience = true,      
                    ValidAudience = _audience,     

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
