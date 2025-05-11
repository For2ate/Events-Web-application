using EventApp.Api.Configurations;
using EventApp.Api.Middleware;
using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;


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

builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();

//builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "EventApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new() {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });


    
});

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
    //app.MapOpenApi();
    app.UseSwagger();
    app.MapScalarApiReference(options => {

        options.Theme = ScalarTheme.BluePlanet;
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");

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
