namespace EventPulse.DAL.Repositories.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveAsync();
}
