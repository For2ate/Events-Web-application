using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventApp.Data.Repositories {
    
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity {

        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(DbContext context) {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id) {
            var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            return entity;
        }

        public async Task <IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int? skip = null,
            int? take = null) {

            IQueryable<TEntity> query = _dbSet;

            query = query.AsNoTracking();

            if (filter != null) {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null) {
                query = orderBy(query);
            } 

            if (skip.HasValue) {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue) {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();

        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null) {

            IQueryable<TEntity> query = _dbSet;

            query = query.AsNoTracking();

            if (filter != null) {
                query = query.Where(filter);
            }

            return await query.CountAsync();

        }

        public async Task AddAsync(TEntity entity) {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity) {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(TEntity entity) {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync() {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task UseTransactionAsync(IDbContextTransaction transaction) {
            await _context.Database.UseTransactionAsync(transaction.GetDbTransaction());
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction) {
            await transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction) {
            await transaction.RollbackAsync();
        }

    }

}
