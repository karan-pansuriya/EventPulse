export interface AdminBookingResponse {
    id: number;
    userId: number;
    customerName: string;
    customerEmail: string;
    eventId: number;
    eventTitle: string;
    venueName: string;
    quantity: number;
    totalAmount: number;
    paymentStatus: string;
    uniqueCode: string;
    createdAt: string;
  }
  