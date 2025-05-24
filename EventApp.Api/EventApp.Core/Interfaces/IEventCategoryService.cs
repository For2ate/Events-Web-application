using EventApp.Models.EventCategoriyDTO.Request;
using EventApp.Models.EventCategoriyDTO.Response;
using EventApp.Models.EventCategoryDTO.Request;
using EventApp.Models.SharedDTO;

namespace EventApp.Core.Interfaces {

    public interface IEventCategoryService {

        Task<PagedListResponse<EventCategoryFullResponseModel>> GetAllCategoriesAsync(EventCategoryPagedQueryParametrs parametrs);

        Task<EventCategoryFullResponseModel?> GetCategoryByIdAsync(Guid id);

        Task<EventCategoryFullResponseModel> CreateCategoryAsync(CreateEventCategoryRequestModel model);

        Task<EventCategoryFullResponseModel?> UpdateCategoryAsync(UpdateEventCategoryRequestModel model);
    
        Task DeleteCategoryAsync(Guid id);

    }

}
