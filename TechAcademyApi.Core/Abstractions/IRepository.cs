namespace TechAcademyApi.Core.Abstractions
{
    /// <summary>
    /// Generic Repository Interface - Abstraction for data persistence
    /// Defined in Core layer to represent the contract between
    /// Use Cases layer and Infrastructure layer
    /// Implements Dependency Inversion Principle
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
