using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Event;

public class CreateEventDto
{
    public int? CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    [RegularExpression(@"^[A-Za-z0-9\s.,'\-]{2,150}$", ErrorMessage = "Venue name must be 2-150 characters and valid symbols only.")]
    public string VenueName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^[A-Za-z0-9\s.,#'\-]{2,200}$", ErrorMessage = "Venue address must be 2-200 characters and valid symbols only.")]
    public string VenueAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[A-Za-z\s'\-]{2,100}$", ErrorMessage = "City must be 2-100 letters.")]
    public string VenueCity { get; set; } = string.Empty;

    [MaxLength(100)]
    [RegularExpression(@"^[A-Za-z\s'\-]{0,100}$", ErrorMessage = "State must be letters only.")]
    public string? VenueState { get; set; }

    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^[A-Za-z\s'\-]{2,100}$", ErrorMessage = "Country must be 2-100 letters.")]
    public string VenueCountry { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^[A-Za-z0-9\s.,'\-]{2,200}$", ErrorMessage = "Title must be 2-200 characters and valid symbols only.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    [RegularExpression(@"^[A-Za-z0-9\s.,'\-]{0,100}$", ErrorMessage = "Genre must be 0-100 characters and valid symbols only.")]
    public string? Genre { get; set; }

    [MaxLength(10)]
    [RegularExpression(@"^[0-9+]{0,10}$", ErrorMessage = "Age restriction must be numeric.")]
    public string? AgeRestriction { get; set; }

    [MaxLength(200)]
    public string? Performers { get; set; }

    [Range(1, 420, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int? DurationMins { get; set; }

    [Required]
    [Range(typeof(DateTime), "2020-01-01", "2100-12-31", ErrorMessage = "Event date is out of range.")]
    public DateTime EventDate { get; set; }

    [Required]
    [Range(typeof(TimeSpan), "00:00:00", "23:59:59", ErrorMessage = "Start time must be between 00:00 and 23:59.")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalSeats { get; set; }
}
