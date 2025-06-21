using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Travel_and_Accommodation_API.DataAccess.Repositories.IRepository;
using Travel_and_Accommodation_API.Helpers;

namespace Travel_and_Accommodation_API.DataAccess.Repositories.RepositoryImplementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly TravelAndAccommodationContext _dbContext;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(TravelAndAccommodationContext dbContext,
            ILogger<Repository<T>> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                _dbContext.Set<T>().Add(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding entity.");
                throw;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            try
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating entity.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(Paging paging)
        {
            try
            {
                return await _dbContext.Set<T>()
                        .Skip((paging.PageNumber - 1) * paging.PageSize)
                        .Take(paging.PageSize)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all entities.");
                throw;
            }
        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _dbContext.Set<T>().FirstOrDefaultAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving entity by filter.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetFilteredItemsAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _dbContext.Set<T>().Where(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving the first entity.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetFilteredItemsAsync(CustomExpression<T> expression)
        {
            try
            {
                IQueryable<T> query = _dbContext.Set<T>();

                if (expression.Filter != null)
                {
                    query = query.Where(expression.Filter);
                }

                if (expression.Sort != null)
                {
                    if (expression.Desc)
                    {
                        query = query.OrderByDescending(expression.Sort);
                    }
                    else
                    {
                        query = query.OrderBy(expression.Sort);
                    }
                }

                if (expression.Paging != null)
                {
                    int pageNumber = expression.Paging.PageNumber;
                    int pageSize = expression.Paging.PageSize;
                    query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving filtered entities.");
                throw;
            }
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                var entitiesToDelete = await _dbContext.Set<T>().Where(filter).ToListAsync();
                _dbContext.Set<T>().RemoveRange(entitiesToDelete);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting entities.");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _dbContext.Set<T>().AnyAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if entities exist.");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _dbContext.Set<T>().Remove(entity);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting entity by Id.");
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbContext.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving entity by Id.");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _dbContext.Set<T>().AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if entity by Id exists.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> AddAllAsync(IEnumerable<T> entities)
        {
            try
            {
                _dbContext.Set<T>().AddRange(entities);
                await _dbContext.SaveChangesAsync();
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding multiple entities.");
                throw;
            }
        }

        public async Task<IEnumerable<T>> DeleteAllAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                var entitiesToDelete = await _dbContext.Set<T>().Where(filter).ToListAsync();
                _dbContext.Set<T>().RemoveRange(entitiesToDelete);
                await _dbContext.SaveChangesAsync();
                return entitiesToDelete;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting multiple entities.");
                throw;
            }
        }
    }
}
