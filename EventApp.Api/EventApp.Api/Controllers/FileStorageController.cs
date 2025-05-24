using EventApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileStorageController : ControllerBase {

        private readonly IFileStorageService _fileStorageService;

        public FileStorageController(IFileStorageService fileStorageService) {

            _fileStorageService = fileStorageService;

        }

        [HttpPost("upload/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(IFormFile file) {

            var imageUrl = await _fileStorageService.SaveFileAsync(file);

            return Ok(imageUrl);

        }

    }

}
