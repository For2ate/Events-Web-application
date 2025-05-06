using EventApp.Data.Entities;

namespace EventApp.Data.Interfaces {

    public interface IEventRegistrationRepository : IBaseRepository<EventRegistrationEntity>{

        Task<EventRegistrationEntity> ExistsRegistrationAsync(Guid userId, Guid eventId);

        Task<IEnumerable<EventRegistrationEntity>> GetParticipantsAsync(Guid eventId);

    }

}
