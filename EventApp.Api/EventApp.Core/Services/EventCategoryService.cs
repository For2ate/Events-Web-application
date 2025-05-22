using AutoMapper;
using EventApp.Core.Exceptions;
using EventApp.Core.Interfaces;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventCategoriyDTO.Request;
using EventApp.Models.EventCategoriyDTO.Response;
using EventApp.Models.EventCategoryDTO.Request;
using EventApp.Models.SharedDTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventApp.Core.Services {

    public class EventCategoryService : IEventCategoryService {

        private readonly IEventCategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public EventCategoryService(IEventCategoryRepository categoryRepository, IMapper mapper) {

            _categoryRepository = categoryRepository;
            _mapper = mapper;

        }

        public async Task<EventCategoryFullResponseModel?> GetCategoryByIdAsync(Guid id) {

            if (id == Guid.Empty) {
                throw new ArgumentException("Id is required");
            }

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null) {
                throw new NotFoundException("category", id);
            }

            return _mapper.Map<EventCategoryFullResponseModel>(category);
        }

        public async Task<PagedListResponse<EventCategoryFullResponseModel>> GetAllCategoriesAsync(
            EventCategoryPagedQueryParametrs queryParameters
            ) {

            Expression<Func<EventCategoryEntity, bool>>? filterExpression = null;

            if (!string.IsNullOrWhiteSpace(queryParameters.NameContains)) {
                string searchTerm = queryParameters.NameContains.Trim().ToLowerInvariant();
                filterExpression = category => category.Name.ToLowerInvariant().Contains(searchTerm);
            }

            Func<IQueryable<EventCategoryEntity>, IOrderedQueryable<EventCategoryEntity>>? orderByFunc = null;
            bool isDescending = queryParameters.SortOrder == SortOrderEnum.desc;

            switch (queryParameters.SortBy) {

                case EventCategorySortByEnum.Name:
                    orderByFunc = q => q.OrderBy(c => c.Name);
                    break;

                default: 
                    orderByFunc = q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name);
                    break;

            }

            var categoriesEntities = await _categoryRepository.GetAllAsync(
                filter: filterExpression,
                orderBy: orderByFunc,
                skip: ( queryParameters.PageNumber - 1 ) * queryParameters.PageSize,
                take: queryParameters.PageSize
            );

            int count = await _categoryRepository.CountAsync(filterExpression);

            var categories =  _mapper.Map<IEnumerable<EventCategoryFullResponseModel>>(categoriesEntities);

            return new PagedListResponse<EventCategoryFullResponseModel>(
                categories,
                queryParameters.PageNumber,
                queryParameters.PageSize,
                count
            );

        }

        public async Task<EventCategoryFullResponseModel> CreateCategoryAsync(CreateEventCategoryRequestModel model) {

            var categoryEntity = _mapper.Map<EventCategoryEntity>(model);
            categoryEntity.Id = Guid.NewGuid();

            try {

                await _categoryRepository.AddAsync(categoryEntity);

                var createdEntity = await _categoryRepository.GetByIdAsync(categoryEntity.Id);

                return _mapper.Map<EventCategoryFullResponseModel>(createdEntity);

            } catch (DbUpdateException ex) {

                if (ex.InnerException != null) {
                    throw new ConflictException($"Category with name '{categoryEntity.Name}' already exists.");
                }

                throw new OperationFailedException("Failed to create category due to a database error.", ex);
            
            } catch (Exception ex) {
                throw new OperationFailedException("An unexpected error occurred while creating the category.", ex);
            }

        }

        public async Task<EventCategoryFullResponseModel?> UpdateCategoryAsync(UpdateEventCategoryRequestModel model) {
            
            if (model.Id == Guid.Empty) {
                throw new ArgumentException("Id is required");
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(model.Id);
            if (existingCategory == null) {
                throw new NotFoundException("category", model);
            }

            _mapper.Map(model, existingCategory);

            await _categoryRepository.UpdateAsync(existingCategory);

            return _mapper.Map<EventCategoryFullResponseModel>(existingCategory);

        }

        public async Task DeleteCategoryAsync(Guid id) {

            var categoryToDelete = await _categoryRepository.GetByIdAsync(id);
            if (categoryToDelete == null) {
                throw new NotFoundException("category", id);
            }

            if (categoryToDelete.Events != null && categoryToDelete.Events.Any()) {
                throw new InvalidOperationException($"Cannot delete category '{categoryToDelete.Name}' (ID: {id}) because it has associated events.");
            }

            try {
         
                await _categoryRepository.RemoveAsync(categoryToDelete);
            
            } catch (Exception ex) {
            
                throw new OperationFailedException("An unexpected error occurred while deleting the category.", ex);
            
            }

        }
        
    }


}
