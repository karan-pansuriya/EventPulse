namespace EventPulse.BLL.DTOs.Event;

public class EventListResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int? VenueId { get; set; }
    public string? VenueName { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public bool IsVerified { get; set; }
    public string? PosterUrl { get; set; }
}
