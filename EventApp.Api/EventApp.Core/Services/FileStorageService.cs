using EventApp.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventApp.Core.Services {

    public class FileStorageService : IFileStorageService {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileStorageService> _logger;

        private readonly string _baseStoragePath;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const string ImageUrlSegment = "/images";

        public FileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<FileStorageService> logger) {

            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

            _baseStoragePath = _webHostEnvironment.WebRootPath;

        }

        public async Task<string?> SaveFileAsync(IFormFile file) {

            try {

                if (file == null || file.Length == 0) {
                    throw new ArgumentException("File is null");
                }

                if (file.Length > MaxFileSize) {
                    throw new ArgumentException($"File size exceeds the limit of {MaxFileSize / 1024 / 1024} MB.");
                }

                if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant())) {  
                    throw new ArgumentException($"Invalid file type: {file.ContentType}. Allowed types are: {string.Join(", ", AllowedImageTypes)}");
                }

                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                var filePath = Path.Combine(_baseStoragePath + "/images", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create)) {
                    await file.CopyToAsync(stream);
                }

                var relativeUrlPath = $"{ImageUrlSegment}/{uniqueFileName}";

                return relativeUrlPath;

            } catch (Exception ex) {

                _logger.LogError(ex, "Error while add file");

                throw;

            }

        }

    }

}
