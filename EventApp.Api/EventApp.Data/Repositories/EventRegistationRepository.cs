using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Data.Repositories {

    public class EventRegistationRepository : BaseRepository<EventRegistrationEntity>, IEventRegistrationRepository {

        public EventRegistationRepository(ApplicationContext context) : base(context) { }

        public async Task<EventRegistrationEntity> ExistsRegistrationAsync(Guid userId, Guid eventId) {

            var registration = await _dbSet.AsNoTracking()
                                           .FirstOrDefaultAsync(er => er.UserId == userId && er.EventId == eventId);

            return registration;

        }

        public async Task<IEnumerable<EventRegistrationEntity>> GetParticipantsAsync(Guid eventId) {

            IQueryable<EventRegistrationEntity> query = _dbSet;

            query = query.AsNoTracking();

            query = query.Where(r => r.EventId == eventId);
            query = query.Include(r => r.User);

            return await query.ToListAsync();

        }

    }

}
