using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventApp.Data.Interfaces {
    
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity {

        Task<TEntity> GetByIdAsync(Guid id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task RemoveAsync(TEntity entity);

        Task<IDbContextTransaction> BeginTransactionAsync();

        Task UseTransactionAsync(IDbContextTransaction transaction);

        Task CommitTransactionAsync(IDbContextTransaction transaction);

        Task RollbackTransactionAsync(IDbContextTransaction transaction);

    }

}
