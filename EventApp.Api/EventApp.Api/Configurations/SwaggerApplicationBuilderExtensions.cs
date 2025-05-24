using Scalar.AspNetCore;

namespace EventApp.Api.Configurations {
    
    public static class SwaggerApplicationBuilderExtensions {

        public static IApplicationBuilder UseApplicationSwagger(this WebApplication app) {

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.MapScalarApiReference(options => {
                    options.Theme = ScalarTheme.BluePlanet;
                    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
                });
            }

            return app;
        
        }

    }

}
