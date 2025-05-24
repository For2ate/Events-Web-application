using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Api.Configurations {
    
    public static class DatabaseMigrationExtensions {

        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app) {

            try {
                
                using (var scope = app.Services.CreateScope()) {

                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Attempting to apply database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");

                }

            } catch (Exception ex) {
                
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            
            }
        }


    }

}
