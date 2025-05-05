using EventApp.Api.Core.Interfaces;
using EventApp.Models.EventCategoryDTO.Request;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] 
    public class EventCategoryController : ControllerBase {

        private readonly IEventCategoryService _categoryService;

        public EventCategoryController(IEventCategoryService categoryService) {
            
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories() {
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            return Ok(categories);
        
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCategoryById(Guid id) {
           
            var category = await _categoryService.GetCategoryByIdAsync(id);
           
            if (category == null) {
                return NotFound($"Category with ID {id} not found.");
            }
            
            return Ok(category);
        
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateEventCategoryRequestModel model) {
            
            try {
                
                var createdCategory = await _categoryService.CreateCategoryAsync(model);
                
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
           
            } catch (ArgumentException ex) {
                
                return BadRequest(ex.Message);
            
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateEventCategoryRequestModel model) {
           
            if (model.Id == Guid.Empty) {
                return BadRequest("Category ID is required for update.");
            }

            try {
                
                var updatedCategory = await _categoryService.UpdateCategoryAsync(model);
                
                if (updatedCategory == null) {
                    
                    return NotFound($"Category with ID {model.Id} not found for update.");
                
                }

                return Ok(updatedCategory);

            } catch (ArgumentException ex) {

                return BadRequest(ex.Message);

            }

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id) {
            
            try {
                
                var success = await _categoryService.DeleteCategoryAsync(id);
                
                if (!success) {
                    return NotFound($"Category with ID {id} not found.");
                }
            
                return NoContent();
            
            } catch (InvalidOperationException ex) {
         
                return BadRequest(ex.Message);
            
            }

        }

    }
}
