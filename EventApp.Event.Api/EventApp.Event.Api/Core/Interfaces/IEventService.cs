using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;

namespace EventApp.Api.Core.Interfaces {

    public interface IEventService {

        Task<EventFullResponseModel?> GetEventByIdAsync(Guid id);

        Task<IEnumerable<EventFullResponseModel>> GetAllEventsAsync();

        Task<EventFullResponseModel> GetEventByNameAsync(string name);

        Task<EventFullResponseModel> CreateEventAsync(CreateEventRequestModel model);

        Task<EventFullResponseModel?> UpdateEventAsync(UpdateEventRequestModel model);

        Task DeleteEventByIdAsync(Guid id);

    }

}

