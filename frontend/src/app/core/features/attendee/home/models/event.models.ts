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

export interface EventDetailResponse {
  id: number;
  organizerId: number;
  organizerName: string | null;
  categoryId: number | null;
  categoryName: string | null;
  venueId: number | null;
  venueName: string | null;
  venueAddress: string | null;
  venueCity: string | null;
  title: string;
  description: string | null;
  genre: string | null;
  ageRestriction: string | null;
  performers: string | null;
  durationMins: number | null;
  eventDate: string;
  startTime: string;
  price: number;
  totalSeats: number;
  isVerified: boolean;
  isActive: boolean;
  posterUrl: string | null;
  posterUrls: string[];
  createdAt: string;
  updatedAt: string | null;
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
