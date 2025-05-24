using EventApp.Api.Configurations;
using EventApp.Api.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services
    .AddApplicationDbContext(builder.Configuration)
    .AddApplicationAutoMapper()
    .AddApplicationServices()                     
    .AddApplicationFluentValidation()
    .AddApplicationJwtAuthentication(builder.Configuration)
    .AddApplicationControllers()
    .AddAuthorization();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync(); 

app.UseApplicationSwagger();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
