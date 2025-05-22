using EventApp.Core.Interfaces;
using EventApp.Models.EventCategoriyDTO.Request;
using EventApp.Models.EventCategoryDTO.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class EventCategoryController : ControllerBase {

        private readonly IEventCategoryService _categoryService;

        public EventCategoryController(IEventCategoryService categoryService) {
            
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] EventCategoryPagedQueryParametrs queryParameters) {

            var pagedResult = await _categoryService.GetAllCategoriesAsync(queryParameters);

            var paginationMetadata = new {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasNextPage,
                pagedResult.HasPreviousPage
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(pagedResult.Items);

        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryById(Guid id) {
           
            var category = await _categoryService.GetCategoryByIdAsync(id);
            
            return Ok(category);
        
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateEventCategoryRequestModel model) {

            var createdCategory = await _categoryService.CreateCategoryAsync(model);

            return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);

        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateEventCategoryRequestModel model) {

            var updatedCategory = await _categoryService.UpdateCategoryAsync(model);

            return Ok(updatedCategory);

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id) {

            await _categoryService.DeleteCategoryAsync(id);

            return NoContent();

        }

    }
}
