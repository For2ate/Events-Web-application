using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Data.Repositories {

    public class EventRepository : BaseRepository<EventEntity>, IEventRepository {

        public EventRepository(ApplicationContext context) : base(context) { }

        public async Task<EventEntity> GetEventByNameAsync(string name) {

            var eventEntity = await _dbSet.FirstOrDefaultAsync(e => e.Name == name);

            return eventEntity;

        }

    }

}
