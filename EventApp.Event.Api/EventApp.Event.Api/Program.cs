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

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseExceptionHandler("/errors");
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
