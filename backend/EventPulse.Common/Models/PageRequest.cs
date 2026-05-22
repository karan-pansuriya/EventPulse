using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventPulse.Common.Models;

public class PageRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
    [DefaultValue(1)]
    public int PageNumber { get; set; }

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    [DefaultValue(10)]
    public int PageSize { get; set; }

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}
