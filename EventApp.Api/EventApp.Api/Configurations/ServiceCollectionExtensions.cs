using EventApp.Api.Core.Interfaces;
using EventApp.Api.Core.Services;
using EventApp.Api.Core.Validation;
using EventApp.Data.Interfaces;
using EventApp.Api.Core.MappingProfilies;
using EventApp.Data.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using EventApp.Api.Core.Resolvers;

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

    }

}
