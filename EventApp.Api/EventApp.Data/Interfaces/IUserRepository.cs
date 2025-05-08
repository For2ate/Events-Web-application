using EventApp.Data.Entities;

namespace EventApp.Data.Interfaces {
 
    public interface IUserRepository : IBaseRepository<UserEntity>{

        Task<UserEntity> GetUserByEmailAsync(string email);

    }

}
