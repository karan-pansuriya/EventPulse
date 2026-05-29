export interface TicketDto {
    id: number;
    ticketCode: string;
    qrCodeUrl: string | null;
    pdfUrl: string | null;
  }
  
  export interface MyTicketResponse {
    bookingId: number;
    bookingCode: string;
    eventTitle: string;
    eventDate: string | null;      // DateOnly serializes as "YYYY-MM-DD"
    venueName: string | null;
    venueCity: string | null;
    quantity: number;
    totalAmount: number;
    createdAt: string;
    tickets: TicketDto[];
  }
  
  export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
  }