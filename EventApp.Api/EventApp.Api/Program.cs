using EventApp.Api.Configurations;
using EventApp.Api.Middleware;
using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

string? connectionStringUserDB = builder.Configuration.GetConnectionString("ApplicationDb");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionStringUserDB));

builder.Services
    .AddApplicationAutoMapper()
    .AddApplicationServices()
    .AddApplicationFluentValidation();

builder.Services.AddApplicationJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

var app = builder.Build();

try {
    using (var scope = app.Services.CreateScope()) {
       
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        await dbContext.Database.MigrateAsync();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); 
        logger.LogInformation("Database migrations applied successfully.");

    }
} catch (Exception ex) {
    var logger = app.Services.GetRequiredService<ILogger<Program>>(); 
    logger.LogError(ex, "An error occurred while migrating the database.");
}


if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference(options => {

        options.Theme = ScalarTheme.BluePlanet;

    });
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();


app.UseAuthorization();

app.MapControllers();

app.Run();
