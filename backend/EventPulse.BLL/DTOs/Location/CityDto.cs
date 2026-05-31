namespace EventPulse.BLL.DTOs.Location;

public class CityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StateId { get; set; }
    public int CountryId { get; set; }
}
