using AutoMapper;
using EventApp.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace EventApp.Core.Resolvers {

    public class ImageUrlResolver : IValueResolver<EventEntity, object, string?> {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageUrlResolver(IHttpContextAccessor httpContextAccessor) {

            _httpContextAccessor = httpContextAccessor;
        
        }

        public string? Resolve(
           EventEntity source,          // Объект-источник (наша EventEntity)
           object destination,          // Объект-назначение (наш DTO, но мы его здесь не используем)
           string? destMember,          // Текущее значение поля назначения до маппинга (обычно null)
           ResolutionContext context    // Контекст маппинга AutoMapper
           ) {

            var relativePath = source.ImageUrl;

            if (string.IsNullOrWhiteSpace(relativePath)) {
                return null; 
            }

            if (Uri.TryCreate(relativePath, UriKind.Absolute, out Uri? uriResult)
                && ( uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps )) {
                return relativePath;
            }

            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null) {

                Console.WriteLine($"Warning: HttpContext is null. Cannot generate absolute URL for relative path: {relativePath}");

                return relativePath;
            }

            HttpRequest request = httpContext.Request;
            string scheme = request.Scheme;  
            string host = request.Host.Value; 

            var safeRelativePath = relativePath.StartsWith('/') ? relativePath : $"/{relativePath}";

            string fullUrl = $"{scheme}://{host}{safeRelativePath}";

            return fullUrl;

        }

    }    

}
