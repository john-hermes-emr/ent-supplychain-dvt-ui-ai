using DVT.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DVT.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DVTContext Context;
        public Repository(DVTContext context) {
            this.Context = context;
        }
        public async Task AddAsync(T entity)
        {
            await Context.Set<T>().AddAsync(entity);
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await Context.Set<T>().AddRangeAsync(entities);
        }
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return Context.Set<T>().Where(predicate);
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            PropertyInfo property = typeof(T).GetTypeInfo().GetDeclaredProperty("Deleted");
            ParameterExpression lambdaArg = Expression.Parameter(typeof(T));
            Expression propertyAccess = Expression.MakeMemberAccess(lambdaArg, property);
            Expression propertyEquals = Expression.Equal(propertyAccess, Expression.Constant(false, typeof(bool)));
            Expression<Func<T, bool>> expressionHere = Expression.Lambda<Func<T, bool>>(propertyEquals, lambdaArg);

            return await Context.Set<T>().Where(expressionHere).ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllNoTrackingAsync()
        {
            PropertyInfo property = typeof(T).GetTypeInfo().GetDeclaredProperty("Deleted");
            ParameterExpression lambdaArg = Expression.Parameter(typeof(T));
            Expression propertyAccess = Expression.MakeMemberAccess(lambdaArg, property);
            Expression propertyEquals = Expression.Equal(propertyAccess, Expression.Constant(false, typeof(bool)));
            Expression<Func<T, bool>> expressionHere = Expression.Lambda<Func<T, bool>>(propertyEquals, lambdaArg);

            return await Context.Set<T>().AsNoTracking().Where(expressionHere).ToListAsync();
        }
        public ValueTask<T> GetByIdAsync(Guid id)
        {
            return Context.Set<T>().FindAsync(id);
        }
        public void Remove(T entity)
        {
            Context.Set<T>().Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            Context.Set<T>().RemoveRange(entities);
        }
        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return Context.Set<T>().SingleOrDefaultAsync(predicate);
        }
    }
}