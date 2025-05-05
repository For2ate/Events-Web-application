using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;

namespace EventApp.Data.Repositories {

    public class EventRegistationRepository : BaseRepository<EventRegistrationEntity>, IEventRegistrationRepository {

        public EventRegistationRepository(ApplicationContext context) : base(context) { }

    }

}
