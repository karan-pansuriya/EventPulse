using EventPulse.DAL.Context;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.DAL.Repositories.Implementations
{
    public class UnitOfWork(EventPulseDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
