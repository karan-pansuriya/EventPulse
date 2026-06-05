export interface EventListResponse {
  id: number;
  title: string;
  categoryName: string | null;
  venueId: number | null;
  venueName: string | null;
  eventDate: string;
  startTime: string;
  price: number;
  totalSeats: number;
  isVerified: boolean;
  isActive: boolean;
  posterUrl: string | null;
}
