using System.Linq.Expressions;

namespace Training_Platform.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null, bool tracked = true
            , Expression<Func<T, object>>[]? includs = null);
        Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, bool tracked = true);
        Task<int> commitAsync();
    }
}
