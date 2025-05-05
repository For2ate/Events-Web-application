using AutoMapper;
using EventApp.Api.Core.Interfaces;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventCategoriyDTO.Response;
using EventApp.Models.EventCategoryDTO.Request;

namespace EventApp.Api.Core.Services {

    public class EventCategoryService : IEventCategoryService {

        private readonly IEventCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public EventCategoryService(IEventCategoryRepository categoryRepository, IMapper mapper) {

            _categoryRepository = categoryRepository;
            _mapper = mapper;

        }

        public async Task<EventCategoryFullResponseModel?> GetCategoryByIdAsync(Guid id) {

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) {
                return null;
            }
            return _mapper.Map<EventCategoryFullResponseModel>(category);
        }

        public async Task<IEnumerable<EventCategoryFullResponseModel>> GetAllCategoriesAsync() {

            var categories = await _categoryRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<EventCategoryFullResponseModel>>(categories);

        }

        public async Task<EventCategoryFullResponseModel> CreateCategoryAsync(CreateEventCategoryRequestModel model) {

            var categoryEntity = _mapper.Map<EventCategoryEntity>(model);
            categoryEntity.Id = Guid.NewGuid();

            await _categoryRepository.AddAsync(categoryEntity);

            var createdEntity = await _categoryRepository.GetByIdAsync(categoryEntity.Id);

            return _mapper.Map<EventCategoryFullResponseModel>(createdEntity);
        }

        public async Task<EventCategoryFullResponseModel?> UpdateCategoryAsync(UpdateEventCategoryRequestModel model) {
            
            var existingCategory = await _categoryRepository.GetByIdAsync(model.Id);
            if (existingCategory == null) {
                return null; 
            }

            _mapper.Map(model, existingCategory);

            await _categoryRepository.UpdateAsync(existingCategory);

            return _mapper.Map<EventCategoryFullResponseModel>(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id) {

            var categoryToDelete = await _categoryRepository.GetByIdAsync(id);
            if (categoryToDelete == null) {
                return false; 
            }


            if (categoryToDelete.Events != null && categoryToDelete.Events.Any()) {
              throw new InvalidOperationException($"Cannot delete category '{categoryToDelete.Name}' (ID: {id}) because it has associated events.");
            }

            await _categoryRepository.RemoveAsync(categoryToDelete); 
            return true;

        }
        
    }


}
