using EventApp.Models.EventCategoriyDTO.Response;
using EventApp.Models.EventCategoryDTO.Request;

namespace EventApp.Api.Core.Interfaces {

    public interface IEventCategoryService {

        Task<IEnumerable<EventCategoryFullResponseModel>> GetAllCategoriesAsync();

        Task<EventCategoryFullResponseModel?> GetCategoryByIdAsync(Guid id);

        Task<EventCategoryFullResponseModel> CreateCategoryAsync(CreateEventCategoryRequestModel model);

        Task<EventCategoryFullResponseModel?> UpdateCategoryAsync(UpdateEventCategoryRequestModel model);
    
        Task<bool> DeleteCategoryAsync(Guid id);

    }

}
