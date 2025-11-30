using ProjArqsi.Domain.Shared;

namespace ProjArqsi.Infrastructure.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        public async Task<int> CommitAsync()
        {
            // For in-memory implementation, nothing to commit
            return await Task.FromResult(0);
        }
    }
}
