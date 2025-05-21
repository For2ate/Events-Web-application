using EventApp.Api.Configurations;
using EventApp.Api.Middleware;
using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services
    .AddApplicationDbContext(builder.Configuration)
    .AddApplicationAutoMapper()
    .AddApplicationServices()                     
    .AddApplicationFluentValidation()
    .AddApplicationJwtAuthentication(builder.Configuration)
    .AddApplicationControllers();

builder.Services.AddAuthorization();

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
