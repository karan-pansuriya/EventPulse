namespace EventPulse.Common.Models;

public class EventFilterRequest : PageRequest
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public int? CategoryId { get; set; }
    
    public string? City { get; set; }
}
