using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Training_Platform.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            //_context; = new();
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // CRUD

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }


        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<int> CommitAsync(
        CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            
            if (expression is not null)
                query = query.Where(expression);
            
            if (includes is not null)
                foreach (var item in includes)
                    if (item is not null)
                        query = query.Include(item);
            
            if (!tracked)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default)
        {
            return (await GetAsync(expression, includes, tracked, cancellationToken)).FirstOrDefault();
        }
    }
}

