namespace EventPulse.BLL.DTOs.Event;

public class EventResponse
{
    public int Id { get; set; }
    public int OrganizerId { get; set; }
    public string? OrganizerName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? VenueId { get; set; }
    public string? VenueName { get; set; }
    public string? VenueAddress { get; set; }
    public string? VenueCity { get; set; }
    public string? VenueState { get; set; }
    public string? VenueCountry { get; set; }
    public int? CityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? AgeRestriction { get; set; }
    public string? Performers { get; set; }
    public int? DurationMins { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public bool IsVerified { get; set; }
    
    // public string? PosterUrl { get; set; }
    public List<string> PosterUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
