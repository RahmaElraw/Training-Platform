using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Training_Platform.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public async Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null, bool tracked = true
            , Expression<Func<T, object>>[]? includes = null)
        {
            var query = _dbSet.AsQueryable();

            if (!tracked)
                query = query.AsNoTracking();

            if (expression is not null)
                query = query.Where(expression);

            if (includes is not null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.ToListAsync();
        }
        public async Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, bool tracked = true)
        {
            return (await GetAsync(expression,tracked)).FirstOrDefault();
        }
        public async Task<int> commitAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                 Console.WriteLine($"An error occurred while saving changes: {ex.Message}");
                return 0;
            }
        }
    }
}

