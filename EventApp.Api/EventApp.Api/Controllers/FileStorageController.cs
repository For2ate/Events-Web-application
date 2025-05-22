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

            if (file == null || file.Length == 0) {
                return BadRequest("No file uploaded or file is empty.");
            }

            try {

                var imageUrl = await _fileStorageService.SaveFileAsync(file);

                if (imageUrl == null) {
                    return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while saving the image.");
                }

                return Ok(imageUrl);

            } catch (ArgumentException ex) {

                return BadRequest(ex.Message);
            
            } catch (Exception ex) {
     
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred while uploading the file.");
            
            }

        }

    }

}
