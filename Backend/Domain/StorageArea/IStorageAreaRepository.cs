using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Domain.StorageAreaAggregate
{
    public interface IStorageAreaRepository : IRepository<StorageArea, StorageAreaId>
    {
        Task<StorageArea> FindByNameAsync(AreaName areaName);
    }
}
