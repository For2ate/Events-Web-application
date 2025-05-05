using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventApp.Api.Configurations {

    public static class JwtExtensions {

        public static IServiceCollection AddApplicationJwtAuthentificate(this IServiceCollection services, IConfiguration configuration) {

            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters {

                    ValidateLifetime = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                    ValidateIssuerSigningKey = true,

                    ClockSkew = TimeSpan.Zero 
                };
            });


            return services;

        }

    }

}
