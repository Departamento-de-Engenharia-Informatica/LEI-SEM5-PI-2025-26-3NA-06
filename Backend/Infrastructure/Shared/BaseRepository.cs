using Microsoft.EntityFrameworkCore;
using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Infrastructure.Shared
{
    public class BaseRepository<TEntity,TEntityId> : IRepository<TEntity,TEntityId>
    where TEntity : Entity<TEntityId>, IAggregateRoot
    where TEntityId : EntityId
    {
        private readonly DbSet<TEntity> _objs;
        
        public BaseRepository(DbSet<TEntity> objs)
        {
            _objs = objs ?? throw new ArgumentNullException(nameof(objs));
        
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _objs.ToListAsync();
        }
        
        public async Task<TEntity?> GetByIdAsync(TEntityId id)
        {
            //return await this._context.Categories.FindAsync(id);
            return await _objs
                .Where(x => id.Equals(x.Id)).FirstOrDefaultAsync();
        }
        public async Task<List<TEntity>> GetByIdsAsync(List<TEntityId> ids)
        {
            return await _objs
                .Where(x => ids.Contains(x.Id)).ToListAsync();
        }
        public async Task<TEntity> AddAsync(TEntity obj)
        {
            var ret = await _objs.AddAsync(obj);
            return ret.Entity;
        }

        public void Remove(TEntity obj)
        {
            _objs.Remove(obj);
        }

        public async Task<TEntity> UpdateAsync(TEntity obj)
        {
            var entry = _objs.Update(obj);
            // If using a DbContext, you may need to call SaveChangesAsync externally
            return await Task.FromResult(entry.Entity);
        }

        public async Task DeleteAsync(TEntityId id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _objs.Remove(entity);
            }
            // If using a DbContext, you may need to call SaveChangesAsync externally
        }
    }
}