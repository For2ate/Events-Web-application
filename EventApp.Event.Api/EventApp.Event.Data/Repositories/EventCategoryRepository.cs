using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;

namespace EventApp.Data.Repositories {
 
    public class EventCategoryRepository : BaseRepository<EventCategoryEntity>, IEventCategoryRepository {

        public EventCategoryRepository(ApplicationContext context) : base(context) { }

    }

}
