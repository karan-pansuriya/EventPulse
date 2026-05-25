namespace EventPulse.BLL.Interfaces;

public interface IVenueService
{
    Task<List<string>> GetCitiesAsync();
}