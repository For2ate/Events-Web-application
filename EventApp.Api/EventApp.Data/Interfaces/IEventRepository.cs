using EventApp.Data.Entities;
using EventApp.Models.EventDTO.Request;

namespace EventApp.Data.Interfaces {

    public interface IEventRepository : IBaseRepository<EventEntity> {

        Task<EventEntity> GetEventByNameAsync(string name);

    }

}
