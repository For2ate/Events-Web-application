using AutoMapper;
using EventApp.Api.Core.Interfaces;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventRegistrationDTO.Request;
using EventApp.Models.EventRegistrationDTO.Response;

namespace EventApp.Api.Core.Services {

    public class EventRegistrationService : IEventRegistrationService {

        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EventRegistrationService> _logger;

        public EventRegistrationService(
            IEventRegistrationRepository eventRegistrationRepository, 
            IMapper mapper, ILogger<EventRegistrationService> logger, 
            IUserRepository userRepository, IEventRepository eventRepository
            ) {

            _logger = logger;
            _eventRegistrationRepository = eventRegistrationRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _eventRepository = eventRepository;
        }

        public async Task<EventRegistrationFullResponseModel?> GetRegistrationByIdAsync(Guid registrationId) {
            var registration = await _eventRegistrationRepository.GetByIdAsync(registrationId);
            if (registration == null) {
                return null;
            }
            return _mapper.Map<EventRegistrationFullResponseModel>(registration);
        }

        public async Task<IEnumerable<ParticipantResponseModel>> GetParticipantsAsync(Guid eventId) {

            try {

                var eventExists = await _eventRepository.GetByIdAsync(eventId);
                if (eventExists == null) {
                    throw new KeyNotFoundException($"Event with ID {eventId} not found.");
                }

                var participants = await _eventRegistrationRepository.GetParticipantsAsync(eventId);

                return _mapper.Map<IEnumerable<ParticipantResponseModel>>(participants);


            } catch (Exception ex) {

                _logger.LogError(ex, "Error while get participants {eventId}", eventId);

                throw;

            }

        }

        public async Task<EventRegistrationFullResponseModel> RegisterUserForEventAsync(Guid userId, RegisterUserForEventRequestModel model) {

            using var transaction = await _eventRegistrationRepository.BeginTransactionAsync();

            try {

                var userExists = await _userRepository.GetByIdAsync(userId);
                if (userExists == null) {
                    throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
                }

                var eventEntity = await _eventRepository.GetByIdAsync(model.EventId); 
                if (eventEntity == null) {
                    throw new ArgumentException($"Event with ID {model.EventId} not found.", nameof(model.EventId));
                }

                if (eventEntity.DateOfEvent < DateTime.UtcNow) {
                    throw new InvalidOperationException("Cannot register for an event that has already passed.");
                }

                if (eventEntity.CurrentNumberOfParticipants >= eventEntity.MaxNumberOfParticipants) {
                    throw new InvalidOperationException($"Event '{eventEntity.Name}' is full. Maximum participants: {eventEntity.MaxNumberOfParticipants}.");
                }

                var alreadyRegistered = await _eventRegistrationRepository.ExistsRegistrationAsync(userId, model.EventId);
                if (alreadyRegistered != null) {
                    throw new InvalidOperationException($"User {userId} is already registered for event {model.EventId}.");
                }

                var registrationEntity = new EventRegistrationEntity {
                    UserId = userId,
                    EventId = model.EventId,
                    RegistrationDate = DateTime.UtcNow
                };

                await _eventRegistrationRepository.AddAsync(registrationEntity);

                eventEntity.CurrentNumberOfParticipants++;

                await _eventRepository.UpdateAsync(eventEntity);

                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} successfully registered for event {EventId}. Registration ID: {RegistrationId}", userId, model.EventId, registrationEntity.Id);

                var response = _mapper.Map<EventRegistrationFullResponseModel>(registrationEntity);
              
                response.UserName = userExists.FirstName; 
                response.UserEmail = userExists.Email; 
                response.EventName = eventEntity.Name;

                return response;

            } catch (Exception ex) {

                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error while register user for event {model}", model);

                throw;

            }

        }

        public async Task<bool> CancelUserRegistrationAsync(Guid userId, Guid eventId) {

            using var transaction = await _eventRegistrationRepository.BeginTransactionAsync();

            try {


                var registration = await _eventRegistrationRepository.ExistsRegistrationAsync(userId, eventId);
                if (registration == null) {          
                    return false; 
                }

                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null) {
                    throw new ArgumentException($"Event with ID {eventId} not found.", nameof(eventId));
                }

                if (eventEntity.DateOfEvent < DateTime.UtcNow) {
                    throw new InvalidOperationException("Cannot cancel registration for an event that has already passed.");
                }

                await _eventRegistrationRepository.RemoveAsync(registration);

                eventEntity.CurrentNumberOfParticipants = Math.Max(0, eventEntity.CurrentNumberOfParticipants - 1);

                await _eventRepository.UpdateAsync(eventEntity);

                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} successfully cancelled registration for event {EventId}. Registration ID: {RegistrationId}", userId, eventId, registration.Id);

                return true;

            } catch (Exception ex) {

                await transaction.RollbackAsync();

                _logger.LogError(ex, "Error while cancel registration user {userId} for event {eventId}", userId, eventId);

                throw;

            }

        }


    }

}
