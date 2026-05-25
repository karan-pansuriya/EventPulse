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

export interface EventFilterRequest {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: string;
  search?: string;
  dateFrom?: string;
  dateTo?: string;
  categoryId?: number;
  city?: string;
}
