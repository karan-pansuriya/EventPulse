using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Event;

public class UpdateEventDto
{
    public int? CategoryId { get; set; }

    [MaxLength(150)]
    public string? VenueName { get; set; }

    public string? VenueAddress { get; set; }

    [MaxLength(100)]
    public string? VenueCity { get; set; }

    [MaxLength(100)]
    public string? VenueState { get; set; }

    [MaxLength(100)]
    public string? VenueCountry { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    [MaxLength(10)]
    public string? AgeRestriction { get; set; }

    public string? Performers { get; set; }

    public int? DurationMins { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalSeats { get; set; }
}
