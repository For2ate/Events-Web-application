using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.SharedDTO;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EventApp.Data.Repositories {

    public class EventRepository : BaseRepository<EventEntity>, IEventRepository {

        public EventRepository(ApplicationContext context) : base(context) { }

        public async Task<EventEntity> GetEventByNameAsync(string name) {

            var eventEntity = await _dbSet.FirstOrDefaultAsync(e => e.Name == name);

            return eventEntity;

        }

        public async Task<(IEnumerable<EventEntity> Events, int TotalCount)> GetFilteredEventsAsync(
            EventQueryParameters queryParameters) {

            IQueryable<EventEntity> query = _dbSet.AsQueryable();

            query = query.AsNoTracking();

            if (queryParameters.DateFrom.HasValue) {
                query = query.Where(e => e.DateOfEvent >= queryParameters.DateFrom.Value);
            }

            if (queryParameters.DateTo.HasValue) {
                var dateToInclusive = queryParameters.DateTo.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(e => e.DateOfEvent <= dateToInclusive);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.Place)) {
                query = query.Where(e => EF.Functions.Like(e.Place, $"%{queryParameters.Place}%"));
            }

            if (queryParameters.CategoryId.HasValue && queryParameters.CategoryId.Value != Guid.Empty) {
                query = query.Where(e => e.CategoryId == queryParameters.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.NameContains)) {
                query = query.Where(e => EF.Functions.Like(e.Name, $"%{queryParameters.NameContains}%"));
            }

            bool isDescending = queryParameters.SortOrder == SortOrderEnum.desc;
            switch (queryParameters.SortBy) {
                case SortByEnum.name:
                    query = isDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name);
                    break;
                case SortByEnum.date:
                    query = isDescending ? query.OrderByDescending(e => e.DateOfEvent) : query.OrderBy(e => e.DateOfEvent);
                    break;
                case SortByEnum.category:
                    query = isDescending ? query.OrderByDescending(e => e.Category.Name) : query.OrderBy(e => e.Category.Name);
                    break;
                case SortByEnum.place:
                    query = isDescending ? query.OrderByDescending(e => e.Place) : query.OrderBy(e => e.Place);
                    break;
                default:
                    query = query.OrderBy(e => e.DateOfEvent);
                    break;
            }
        

            var totalCount = await query.CountAsync();

            var events = await query
                .Skip(( queryParameters.PageNumber - 1 ) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Include(e => e.Category) 
                .ToListAsync();

            return (events, totalCount);

        }

    }

}
