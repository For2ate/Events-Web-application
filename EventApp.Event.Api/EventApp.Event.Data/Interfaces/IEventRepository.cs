using EventApp.Data.Entities;

namespace EventApp.Data.Interfaces {

    public interface IEventRepository : IBaseRepository<EventEntity> {

        Task<EventEntity> GetEventByNameAsync(string name);

    }

}
