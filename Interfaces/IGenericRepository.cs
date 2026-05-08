

using Microsoft.EntityFrameworkCore;
using MOZ_UPGRADE.Context;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddNew(T entity);
        Task<List<T>> GetAll();
        Task<T> GetById(string Id);
        Task Remove(T entity);
        Task Update(T entity);
    }

    public class GenericRepo<T> : IGenericRepository<T> where T : class
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public GenericRepo(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task AddNew(T entity)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Set<T>().AddAsync(entity);
            dbContext.SaveChanges();
        }

        public async Task<List<T>> GetAll()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetById(string Id)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await dbContext.Set<T>().FindAsync(Id);
        }

        public async Task Remove(T entity)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Set<T>().Update(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}
