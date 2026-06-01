namespace EventPulse.DAL.Repositories.Interfaces;

public interface IVenueRepository
{
    Task<List<string>> GetCityNamesAsync();
}
