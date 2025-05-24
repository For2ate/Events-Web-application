using EventApp.Models.EventRegistrationDTO.Request;
using EventApp.Models.EventRegistrationDTO.Response;

namespace EventApp.Core.Interfaces {

    public interface IEventRegistrationService {

        Task<EventRegistrationFullResponseModel?> GetRegistrationByIdAsync(Guid registrationId);

        Task<IEnumerable<ParticipantResponseModel>> GetParticipantsAsync(Guid eventId);

        Task<EventRegistrationFullResponseModel> RegisterUserForEventAsync(Guid userId, RegisterUserForEventRequestModel model);

        Task<bool> CancelUserRegistrationAsync(Guid userId, Guid eventId);

    }

}
