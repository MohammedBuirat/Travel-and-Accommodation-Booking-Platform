using System.Linq.Expressions;
using Travel_and_Accommodation_API.Helpers;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.IRepository
{
    public interface IRepository<T> where T : class
    {
        public Task<T> AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task<IEnumerable<T>> GetAllAsync(Paging paging);
        public Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        public Task<IEnumerable<T>> GetFilteredItemsAsync(CustomExpression<T> expression);
        public Task DeleteAsync(Expression<Func<T, bool>> filter);
        public Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        public Task<IEnumerable<T>> AddAllAsync(IEnumerable<T> entities);
        public Task DeleteAsync(Guid id);
        public Task<T?> GetByIdAsync(Guid id);
        public Task<bool> ExistsAsync(Guid id);
        public Task<IEnumerable<T>> DeleteAllAsync(Expression<Func<T, bool>> filter);
        public Task<IEnumerable<T>> GetFilteredItemsAsync(Expression<Func<T, bool>> filter);
    }
}
