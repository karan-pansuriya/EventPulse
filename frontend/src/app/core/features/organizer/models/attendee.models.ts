export interface CheckInResponse {
  success: boolean;
  message: string;
  ticketId: number;
  ticketCode: string;
  eventTitle: string;
  attendeeName: string | null;
  checkedInAt: string;
}

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
