using EventApp.Api.Core.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventController : ControllerBase {

        private readonly IEventService _eventService;

        public EventController(IEventService eventService) {

            _eventService = eventService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents() {
            
            var events = await _eventService.GetAllEventsAsync();

            return Ok(events);
        
        }

        [HttpGet("filtered")]
        public async Task<IActionResult> GetEvents([FromQuery] EventQueryParameters queryParameters) {

            var pagedResult = await _eventService.GetFilteredEventsAsync(queryParameters);

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id) {

            var eventModel = await _eventService.GetEventByIdAsync(id);

            if (eventModel == null) {
                return NotFound($"Event with ID {id} not found.");
            }

            return Ok(eventModel);

        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetEventByName(string name) {

            if (string.IsNullOrWhiteSpace(name)) {
                return BadRequest("Event name cannot be empty.");
            }

            try {
                var eventModel = await _eventService.GetEventByNameAsync(name);

                if (eventModel == null) {
                    return NotFound($"Event with name '{name}' not found.");
                }

                return Ok(eventModel);

            } catch (KeyNotFoundException ex) {

                return NotFound(ex.Message);

            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestModel model) {

            try {

                var createdEvent = await _eventService.CreateEventAsync(model);

                return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);

            } catch (ArgumentException ex) {

                return BadRequest(ex.Message);
            }

        }

        [HttpPut] 
        public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequestModel model) {

            try {

                var updatedEvent = await _eventService.UpdateEventAsync(model);

                if (updatedEvent == null) {
                    return NotFound($"Event with ID {model.Id} not found for update.");
                }

                return Ok(updatedEvent);

            } catch (ArgumentException ex) {

                return BadRequest(ex.Message);

            } catch (KeyNotFoundException ex) {

                return NotFound(ex.Message);
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id) {

            try {

                await _eventService.DeleteEventByIdAsync(id);

                return NoContent();

            } catch (KeyNotFoundException ex) {

                return NotFound(ex.Message);

            }

        }

    }

}
