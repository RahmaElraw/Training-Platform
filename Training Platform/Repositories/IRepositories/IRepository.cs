using System.Linq.Expressions;

namespace Training_Platform.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Delete(T entity);
        void Update(T entity);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
