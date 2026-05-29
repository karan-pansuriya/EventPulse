using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventPulse.BLL.Services;

public class TicketGenerationService : ITicketGenerationService
{
    private readonly string _webRootPath;

    public TicketGenerationService(string webRootPath)
    {
        _webRootPath = webRootPath;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task GenerateTicketDocumentsAsync(Booking booking)
    {
        // Each ticket in the booking gets its own separate PDF
        foreach (Ticket ticket in booking.Tickets)
        {
            if (!string.IsNullOrEmpty(ticket.PdfPath))
                continue;

            byte[] qrBytes = GenerateQrCode(ticket.TicketCode);

            string qrRelative = Path.Combine("uploads", "tickets", "qr", $"{Guid.NewGuid()}.png")
                .Replace("\\", "/");
            string qrFull = Path.Combine(_webRootPath, qrRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(qrFull)!);
            await File.WriteAllBytesAsync(qrFull, qrBytes);

            // Pass booking + individual ticket — PDF always shows 1 ticket data
            byte[] pdfBytes = GeneratePdf(booking, ticket, qrBytes);

            string pdfRelative = Path.Combine("uploads", "tickets", "pdf", $"{Guid.NewGuid()}.pdf")
                .Replace("\\", "/");
            string pdfFull = Path.Combine(_webRootPath, pdfRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(pdfFull)!);
            await File.WriteAllBytesAsync(pdfFull, pdfBytes);

            ticket.QrCodePath = qrRelative;
            ticket.PdfPath    = pdfRelative;
        }
    }

    private static byte[] GenerateQrCode(string data)
    {
        using QRCodeGenerator gen = new();
        QRCodeData qrData = gen.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qr = new(qrData);
        return qr.GetGraphic(20);
    }

    private static byte[] GeneratePdf(Booking booking, Ticket ticket, byte[] qrBytes)
    {
        // ── Data — always per single ticket ─────────────────────────────────
        string eventTitle = booking.Event?.Title ?? "Event";
        string eventDate  = booking.Event?.EventDate.ToString("ddd, MMM dd yyyy") ?? "";
        string eventTime  = booking.Event != null
            ? $"{booking.Event.StartTime.Hours:D2}:{booking.Event.StartTime.Minutes:D2}"
            : "";
        string venueName  = booking.Event?.Venue?.Name ?? "";
        string venueCity  = booking.Event?.Venue?.City ?? "";
        string location   = string.Join(", ",
            new[] { venueName, venueCity }.Where(s => !string.IsNullOrWhiteSpace(s)));

        string bookingCode = booking.UniqueCode;
        string ticketCode  = ticket.TicketCode;

        // Each PDF = 1 ticket, so quantity is always 1 and price is per-ticket
        decimal unitPrice = booking.PricePerTicket;
        decimal convFee   = 0m;
        decimal total     = unitPrice + convFee;

        // ── Colors ──────────────────────────────────────────────────────────
        string bgGrey    = "#F0F0F0";
        string black     = "#111111";
        string darkGrey  = "#333333";
        string midGrey   = "#666666";
        string lightLine = "#BBBBBB";
        string stubBg    = "#E8E8E8";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(620, 240, Unit.Point);
                page.Margin(0);
                // No LetterSpacing on default — that caused the spaced-out text bug
                page.DefaultTextStyle(x =>
                    x.FontFamily("Helvetica").FontSize(10).FontColor(black));

                page.Content().Background(bgGrey).Row(mainRow =>
                {
                    // LEFT PANEL — Event details + pricing
                    mainRow.RelativeItem().Padding(28).Column(col =>
                    {
                        // Event name
                        col.Item().PaddingBottom(10)
                            .Text(eventTitle)
                            .Bold().FontSize(18).FontColor(black);

                        // Date
                        col.Item().PaddingBottom(3)
                            .Text($"Date  :  {eventDate}  |  {eventTime}")
                            .FontSize(10).FontColor(darkGrey);

                        // Location
                        col.Item().PaddingBottom(16)
                            .Text($"Location  :  {location}")
                            .FontSize(10).FontColor(darkGrey);

                        // Ticket price row
                        col.Item().PaddingBottom(3)
                                .Text($"Ticket Price  :  Rs. {unitPrice:N2}")
                                .FontSize(10).FontColor(darkGrey);

                    });

                    // ── Vertical separator ───────────────────────────────
                    mainRow.ConstantItem(1).Background(lightLine);

                    // RIGHT STUB — QR + Booking ID
                    mainRow.ConstantItem(175).Background(stubBg)
                        .Padding(20).Column(col =>
                        {
                            // QR code
                            col.Item().AlignCenter().PaddingBottom(8)
                                .Width(90).Height(90)
                                .Image(qrBytes).FitArea();

                            // Label
                            col.Item().AlignCenter().PaddingBottom(2)
                                .Text("Ticket Code")
                                .FontSize(9).FontColor(midGrey);

                            // Short ID
                            col.Item().AlignCenter().PaddingBottom(12)
                                .Text(ticketCode)
                                .Bold().FontSize(13).FontColor(black);
                        });
                });
            });
        }).GeneratePdf();
    }
}