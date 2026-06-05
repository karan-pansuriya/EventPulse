export interface OrganizerDashboardData {
  totalEvents: number;
  upcomingEvents: number;
  totalTicketsSold: number;
  totalRevenue: number;
  totalAttendees: number;
  weeklyComparison: WeeklyComparison;
  topBookedEvents: TopBookedEvent[];
  recentAttendees: RecentAttendeeItem[];
  monthlyRevenue: MonthlyRevenue[];
  eventsByCategory: CategoryEventCount[];
  attendeeInterests: AttendeeInterestItem[];
}

export interface WeeklyComparison {
  eventsChange: number;
  upcomingEventsChange: number;
  ticketsSoldChange: number;
  revenueChange: number;
  attendeesChange: number;
}

export interface TopBookedEvent {
  id: number;
  title: string;
  posterUrl: string | null;
  totalBookings: number;
  revenueGenerated: number;
  eventDate: string;
  organizerId: number;
  organizerName: string;
}

export interface RecentAttendeeItem {
  bookingId: number;
  userId: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  eventId: number;
  eventTitle: string;
  quantity: number;
  totalAmount: number;
  bookedAt: string;
}

export interface MonthlyRevenue {
  month: string;
  revenue: number;
  bookings: number;
}

export interface CategoryEventCount {
  categoryName: string;
  count: number;
}

export interface MonthlyEventCount {
  month: string;
  count: number;
}

export interface AttendeeInterestItem {
  categoryName: string;
  attendeeCount: number;
}
