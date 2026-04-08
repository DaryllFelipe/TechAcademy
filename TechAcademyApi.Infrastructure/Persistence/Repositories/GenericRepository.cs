using Microsoft.EntityFrameworkCore;
using TechAcademyApi.Core.Abstractions;

namespace TechAcademyApi.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Generic Repository Implementation - Infrastructure Layer
    /// Concrete implementation of the repository abstraction defined in Core layer
    /// Implements the Repository pattern for data access abstraction
    /// </summary>
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly TechAcademyDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(TechAcademyDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
