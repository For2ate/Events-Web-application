using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using EventApp.Models.SharedDTO;

namespace EventApp.Core.Interfaces {

    public interface IEventService {

        Task<EventFullResponseModel?> GetEventByIdAsync(Guid id);

        Task<IEnumerable<EventFullResponseModel>> GetAllEventsAsync();

        Task<PagedListResponse<EventFullResponseModel>> GetFilteredEventsAsync(EventQueryParameters queryParameters);

        Task<EventFullResponseModel> GetEventByNameAsync(string name);

        Task<EventFullResponseModel> CreateEventAsync(CreateEventRequestModel model);

        Task<EventFullResponseModel?> UpdateEventAsync(UpdateEventRequestModel model);

        Task DeleteEventByIdAsync(Guid id);

    }

}

