using EventApp.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace EventApp.Data.Interfaces {
    
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity {

        Task<TEntity> GetByIdAsync(Guid id);

        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null);

        Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);

        Task AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task RemoveAsync(TEntity entity);

        Task<IDbContextTransaction> BeginTransactionAsync();

        Task UseTransactionAsync(IDbContextTransaction transaction);

        Task CommitTransactionAsync(IDbContextTransaction transaction);

        Task RollbackTransactionAsync(IDbContextTransaction transaction);

    }

}
