using EventApp.Api.Core.Interfaces;
using EventApp.Api.Core.Services;
using EventApp.Api.Core.Validation;
using EventApp.Data.Interfaces;
using EventApp.Api.Core.MappingProfilies;
using EventApp.Data.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using EventApp.Api.Core.Resolvers;
using Microsoft.OpenApi.Models;
using EventApp.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EventApp.Api.Configurations {

    public static class ServiceCollectionExtensions {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) {

            // Repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
            services.AddScoped<IEventRegistrationRepository, EventRegistationRepository>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventCategoryService, EventCategoryService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IEventRegistrationService, EventRegistrationService>();

            //swagger 
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new() { Title = "EventApp API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new() {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                c.AddSecurityRequirement( new() {
                    {
                        new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        Array.Empty<string>()
                    }
                });

            });

            return services;

        }

        public static IServiceCollection AddApplicationAutoMapper(this IServiceCollection services) {

            services.AddScoped<ImageUrlResolver>();

            services.AddAutoMapper(typeof(UserMappingProfile));
            services.AddAutoMapper(typeof(EventMappingProfile));
            services.AddAutoMapper(typeof(EventCategoryMappingProfile));
            services.AddAutoMapper(typeof(EventRegistationMappingProfile));

            return services;

        }

        public static IServiceCollection AddApplicationFluentValidation(this IServiceCollection services) {

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
            services.AddValidatorsFromAssemblyContaining<UserLoginValidator>();

            return services;

        }

        public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration) {
            
            string? connectionStringUserDB = configuration.GetConnectionString("ApplicationDb");
         
            if (string.IsNullOrEmpty(connectionStringUserDB)) {
                throw new InvalidOperationException("Connection string 'ApplicationDb' not found.");
            }
            
            services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionStringUserDB));
        
            return services;
        
        }

        public static IServiceCollection AddApplicationControllers(this IServiceCollection services) {
            
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddHttpContextAccessor();

            return services;
        }



    }

}
