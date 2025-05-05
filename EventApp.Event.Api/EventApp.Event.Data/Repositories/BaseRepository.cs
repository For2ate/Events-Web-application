using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Data.Repositories {
    
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity {

        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(DbContext context) {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(Guid id) {
            var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() {
            return await _dbSet.ToListAsync();
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
