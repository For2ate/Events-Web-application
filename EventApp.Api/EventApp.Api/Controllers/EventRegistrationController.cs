using EventApp.Core.Interfaces;
using EventApp.Models.EventRegistrationDTO.Request;
using EventApp.Models.EventRegistrationDTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventApp.Api.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventRegistrationController : ControllerBase {

        private readonly IEventRegistrationService _eventRegistrationService;

        public EventRegistrationController(IEventRegistrationService eventRegistrationService) {

            _eventRegistrationService = eventRegistrationService;

        }

        [HttpPost("event")]
        public async Task<IActionResult> RegistationUserForEvent(RegisterUserForEventRequestModel model) {

            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out Guid currentUserId)) {
                return Unauthorized("User ID not found in token or is invalid.");
            }

            var registration = await _eventRegistrationService.RegisterUserForEventAsync(currentUserId, model);
            return Ok(registration);

        }

        [HttpDelete("event/{eventId:guid}")]
        public async Task<IActionResult> CancelUserRegistration(Guid eventId) {

            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out Guid userId)) {
                return Unauthorized("User ID not found in token or is invalid.");
            }

            var success = await _eventRegistrationService.CancelUserRegistrationAsync(userId, eventId);
            return NoContent();

        }

        [HttpGet("event/{eventId:guid}/participants")]
        public async Task<IActionResult> GetEventParticipants(Guid eventId) {

            var participants = await _eventRegistrationService.GetParticipantsAsync(eventId);
            return Ok(participants);

        }

        [HttpGet("event/{registrationId:guid}")]
        public async Task<IActionResult> GetRegistrationById(Guid registrationId) {

            var registration = await _eventRegistrationService.GetRegistrationByIdAsync(registrationId);
            return Ok(registration);

        }

    }

}
