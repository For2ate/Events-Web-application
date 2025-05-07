using EventApp.Api.Configurations;
using EventApp.Api.Middleware;
using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Net;
using System.Security.Cryptography.X509Certificates;


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

Console.WriteLine($"Cert path: {Environment.GetEnvironmentVariable("ASPNETCORE_KESTREL_CERTIFICATES_DEFAULT_PATH")}");
Console.WriteLine($"Cert exists: {File.Exists(Environment.GetEnvironmentVariable("ASPNETCORE_KESTREL_CERTIFICATES_DEFAULT_PATH"))}");
Console.WriteLine(app.Environment.IsDevelopment());


if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference(options => {

        options.Theme = ScalarTheme.BluePlanet;

    });
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();


//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();


app.UseAuthorization();

app.MapControllers();

app.Run();
