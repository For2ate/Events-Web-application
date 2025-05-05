using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventApp.Api.Configurations {

    public static class JwtExtensions {

        public static IServiceCollection AddApplicationJwtAuthentication(this IServiceCollection services, IConfiguration configuration) {
            
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? throw new ArgumentNullException("Jwt:Key");
            var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
            var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Jwt:Audience"); 

            services.AddAuthentication(options => {

                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options => {

                options.TokenValidationParameters = new TokenValidationParameters {

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                    ValidateIssuer = true,         
                    ValidIssuer = issuer,          

                    ValidateAudience = true,       
                    ValidAudience = audience,      

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero

                };

            });

            return services;

        }

    }

}
