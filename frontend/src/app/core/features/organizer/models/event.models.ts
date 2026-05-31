export interface CreateEventRequest {
  categoryId?: number;
  venueName: string;
  venueAddress: string;
  cityId: number;
  title: string;
  description?: string;
  genre?: string;
  ageRestriction?: string;
  performers?: string;
  durationMins?: number;
  eventDate: string;
  startTime: string;
  price: number;
  totalSeats: number;
}

export interface UpdateEventRequest {
  categoryId?: number;
  venueName?: string;
  venueAddress?: string;
  cityId?: number;
  title: string;
  description?: string;
  genre?: string;
  ageRestriction?: string;
  performers?: string;
  durationMins?: number;
  eventDate: string;
  startTime: string;
  price: number;
  totalSeats: number;
  removePosterUrls?: string[];
}
