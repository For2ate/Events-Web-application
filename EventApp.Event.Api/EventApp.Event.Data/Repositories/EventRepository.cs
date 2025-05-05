using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;

namespace EventApp.Data.Repositories {

    public class EventRepository : BaseRepository<EventEntity>, IEventRepository {

        public EventRepository(ApplicationContext context) : base(context) { }

    }

}
