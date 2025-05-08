using EventApp.Api.Core.Interfaces;
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


            try {

                var registration = await _eventRegistrationService.RegisterUserForEventAsync(currentUserId ,model);

                return Ok(registration);

            } catch (ArgumentException ex) { // User/Event not found

                if (ex.ParamName == nameof(currentUserId)) {
                    return NotFound(ex.Message);
                }
                if (ex.ParamName == nameof(model.EventId)) {
                    return NotFound(ex.Message);
                }

                return BadRequest(ex.Message);

            } catch (InvalidOperationException ex) {

                if (ex.Message.Contains("already registered") || ex.Message.Contains("is full")) {
                    return Conflict(ex.Message);
                } else {
                    return BadRequest(ex.Message);
                }

            }

        }

        [HttpDelete("event/{eventId:guid}")]
        public async Task<IActionResult> CancelUserRegistration(Guid eventId) {

            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out Guid userId)) {
                return Unauthorized("User ID not found in token or is invalid.");
            }

            try {

                var success = await _eventRegistrationService.CancelUserRegistrationAsync(userId, eventId);

                if (!success) {
                    return NotFound($"Registration for user on event {eventId} not found or cancellation failed.");
                }

                return NoContent();

            } catch (ArgumentException ex) {

                return NotFound(ex.Message);

            } catch (InvalidOperationException ex) {

                return BadRequest(ex.Message);

            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

            }

        }

        [HttpGet("event/{eventId:guid}/participants")]
        public async Task<IActionResult> GetEventParticipants(Guid eventId) {
            try {

                var participants = await _eventRegistrationService.GetParticipantsAsync(eventId);
                return Ok(participants);

            } catch (KeyNotFoundException ex) {

                return NotFound(ex.Message);

            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

            }

        }

        [HttpGet("event/{registrationId:guid}")]
        public async Task<IActionResult> GetRegistrationById(Guid registrationId) {
            try {

                var registration = await _eventRegistrationService.GetRegistrationByIdAsync(registrationId);
                if (registration == null) {
                    return NotFound($"Registration with ID {registrationId} not found.");
                }

                return Ok(registration);

            } catch (Exception) {

                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");

            }
        }

    }

}
