using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class Repository<T>(ApplicationDbContext _dbContext) : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext = _dbContext;
        private readonly DbSet<T> dbSet = _dbContext.Set<T>();

        public void Add(T entity)
        {
            dbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            dbSet.AddRange(entities);
            _dbContext.SaveChanges();
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>>? predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var item in includeProperties)
            {
                query = query.Include(item);
            }

            return query.ToList();
        }

        public T? GetOne(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            return Get(predicate, includeProperties).FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
            _dbContext.SaveChanges();
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            dbSet.UpdateRange(entities);
            _dbContext.SaveChanges();
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }
    }
}
