using AutoMapper;
using EventApp.Core.Exceptions;
using EventApp.Core.Interfaces;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using EventApp.Models.SharedDTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EventApp.Core.Services {

    public class EventService : IEventService {

        private readonly IEventRepository _eventRepository;
        private readonly IEventCategoryRepository _eventCategoryRepository;
        private readonly IMapper _eventMapper;
        private readonly ILogger<EventService> _logger;

        public EventService( IEventRepository eventRepository, IEventCategoryRepository eventCategoryRepository, 
                            IMapper eventMapper, ILogger<EventService> logger
            ) {

            _eventCategoryRepository = eventCategoryRepository;
            _eventMapper = eventMapper;
            _eventRepository = eventRepository;
            _logger = logger;

        }

        public async Task<EventFullResponseModel> GetEventByIdAsync(Guid id) {

            var eventEntity = await _eventRepository.GetByIdAsync(id);

            if (eventEntity == null) {
                _logger.LogWarning("Event with ID {EventId} not found.", id);
                throw new NotFoundException("Event", id.ToString());
            }

            return _eventMapper.Map<EventFullResponseModel>(eventEntity);

        }

        public async Task<IEnumerable<EventFullResponseModel>> GetAllEventsAsync(
            EventQueryParameters queryParameters) {

            try {

                Expression<Func<EventEntity, bool>>? filterExpression = null;



                var events = await _eventRepository.GetAllAsync();

                return _eventMapper.Map<IEnumerable<EventFullResponseModel>>(events);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error while get all events");

                throw new ArgumentException("No content");

            }

        }

        public async Task <EventFullResponseModel> GetEventByNameAsync(string name) {

            try {

                var eventEntity = await _eventRepository.GetEventByNameAsync(name);
                if (eventEntity == null) {
                    throw new NotFoundException("event", name);
                }

                return _eventMapper.Map<EventFullResponseModel>(eventEntity);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error while get event {name}", name);

                throw;

            } 

        }

        public async Task<EventFullResponseModel> CreateEventAsync(CreateEventRequestModel model) {

            try {

                var categoryExist = await _eventCategoryRepository.GetByIdAsync(model.CategoryId);
                if (categoryExist == null) {
                    throw new ArgumentException($"Category with Id {model.CategoryId} not found.", nameof(model.CategoryId));
                }

                var eventEntity = _eventMapper.Map<EventEntity>(model);
                eventEntity.Id = Guid.NewGuid();

                await _eventRepository.AddAsync(eventEntity);

                var responseDto = _eventMapper.Map<EventFullResponseModel>(eventEntity);

                return responseDto;

            } catch(Exception ex) {

                _logger.LogError(ex, "Error create event {model}", model);

                throw;

            }


        }

        public async Task<EventFullResponseModel?> UpdateEventAsync(UpdateEventRequestModel model) 
        {

            using var transaction = await _eventRepository.BeginTransactionAsync();

            try {

                var existingEntity = await _eventRepository.GetByIdAsync(model.Id);
                if (existingEntity == null) {
                    throw new NotFoundException("Event", model.Id);
                }


                if (model.CategoryId != Guid.Empty && model.CategoryId != existingEntity.CategoryId) {

                    var category = await _eventCategoryRepository.GetByIdAsync(model.CategoryId);

                    if (category == null) {
                        throw new NotFoundException("Category", model.CategoryId);
                    }

                } else {

                    throw new BadRequestException("Ids is required");

                }

                _eventMapper.Map(model, existingEntity);

                existingEntity.Id = model.Id;

                await _eventRepository.UpdateAsync(existingEntity);

                var responseEntity = await _eventRepository.GetByIdAsync(model.Id);

                await transaction.CommitAsync();

                return _eventMapper.Map<EventFullResponseModel>(responseEntity);

            } catch (Exception ex) {

                await transaction.RollbackAsync();

                _logger.LogError(ex, "error while update event {id}", model.Id);

                throw;

            }

        }

        public async Task DeleteEventByIdAsync(Guid id) {

            try {

                var eventEntity = await _eventRepository.GetByIdAsync(id);
                if (eventEntity == null) {
                    throw new NotFoundException("Event", id);
                }

                await _eventRepository.RemoveAsync(eventEntity);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error while delete event {id}", id);

                throw;

            }

        }

    }

}
