using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Data.Repositories {

    public class UserRepository : BaseRepository<UserEntity>, IUserRepository{

        public UserRepository(ApplicationContext context) : base(context) { }

        public async Task<UserEntity> GetUserByEmailAsync(string email) {

            try {

                var user = await _dbSet
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.Email == email);

                return user;

            } catch (Exception ex) {

                return null;

            } 

        }

    }

}
