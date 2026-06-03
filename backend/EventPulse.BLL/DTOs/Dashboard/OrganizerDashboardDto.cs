namespace EventPulse.BLL.DTOs.Dashboard;

public class OrganizerDashboardDto
{
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalAttendees { get; set; }
    public WeeklyComparisonDto WeeklyComparison { get; set; } = new();
    public List<TopBookedEventDto> TopBookedEvents { get; set; } = [];
    public List<DashboardRecentAttendeeDto> RecentAttendees { get; set; } = [];
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = [];
    public List<CategoryEventCountDto> EventsByCategory { get; set; } = [];
}

public class WeeklyComparisonDto
{
    public int EventsChange { get; set; }
    public int UpcomingEventsChange { get; set; }
    public int TicketsSoldChange { get; set; }
    public decimal RevenueChange { get; set; }
    public int AttendeesChange { get; set; }
}

public class TopBookedEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? PosterUrl { get; set; }
    public int TotalBookings { get; set; }
    public decimal RevenueGenerated { get; set; }
    public DateTime EventDate { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = "";
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = "";
    public decimal Revenue { get; set; }
    public int Bookings { get; set; }
}

public class CategoryEventCountDto
{
    public string CategoryName { get; set; } = "";
    public int Count { get; set; }
}

public class MonthlyEventCountDto
{
    public string Month { get; set; } = "";
    public int Count { get; set; }
}

public class DashboardRecentAttendeeDto
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = "";
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }
}
