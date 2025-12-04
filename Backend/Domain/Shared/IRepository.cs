namespace ProjArqsi.Domain.Shared
{
    public interface IRepository<TEntity, TEntityId> 
        where TEntity : IAggregateRoot
        where TEntityId : class
    {
        Task<TEntity?> GetByIdAsync(TEntityId id);
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntityId id);
    }
}
