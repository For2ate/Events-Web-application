using EventApp.Core.Interfaces;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(Guid id) {

            var eventModel = await _eventService.GetEventByIdAsync(id);

            return Ok(eventModel);

        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetEventByName(string name) {

            var eventModel = await _eventService.GetEventByNameAsync(name);

            return Ok(eventModel);

        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestModel model) {

            var createdEvent = await _eventService.CreateEventAsync(model);

            return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);

        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent([FromBody] UpdateEventRequestModel model) {


            var updatedEvent = await _eventService.UpdateEventAsync(model);

            return Ok(updatedEvent);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id) {

            await _eventService.DeleteEventByIdAsync(id);

            return NoContent();

        }

    }

}
