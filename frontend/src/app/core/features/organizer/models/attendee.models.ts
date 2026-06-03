export interface EventAttendee {
  bookingId: number;
  userId: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string | null;
  eventId: number;
  eventTitle: string;
  quantity: number;
  totalAmount: number;
  paymentStatus: string;
  bookedAt: string;
}
