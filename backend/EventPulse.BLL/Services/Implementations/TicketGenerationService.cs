using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventPulse.BLL.Services;

public class TicketGenerationService : ITicketGenerationService
{
    
    private readonly IWebHostEnvironment _environment;

    public TicketGenerationService(IWebHostEnvironment environment)
    {
        _environment = environment;
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
            string qrFull = Path.Combine(_environment.WebRootPath, qrRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(qrFull)!);
            await File.WriteAllBytesAsync(qrFull, qrBytes);

            // Pass booking + individual ticket — PDF always shows 1 ticket data
            byte[] pdfBytes = GeneratePdf(booking, ticket, qrBytes);

            string pdfRelative = Path.Combine("uploads", "tickets", "pdf", $"{Guid.NewGuid()}.pdf")
                .Replace("\\", "/");
            string pdfFull = Path.Combine(_environment.WebRootPath, pdfRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(pdfFull)!);
            await File.WriteAllBytesAsync(pdfFull, pdfBytes);

            ticket.QrCodePath = qrRelative;
            ticket.PdfPath = pdfRelative;
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
        // ── Data ────────────────────────────────────────────────────────────
        string eventTitle = booking.Event?.Title ?? "Event";
        string eventDate = booking.Event?.EventDate.ToString("dddd, MMMM dd yyyy") ?? "";
        string eventTime = booking.Event != null
            ? $"{booking.Event.StartTime.Hours:D2}:{booking.Event.StartTime.Minutes:D2}"
            : "";
        string venueName = booking.Event?.Venue?.Name ?? "";
        string venueCity = booking.Event?.Venue?.City?.Name ?? "";
        string venueAddress = booking.Event?.Venue?.Address ?? "";
        string category = booking.Event?.Category?.Name ?? "";
        string genre = booking.Event?.Genre ?? "";
        string fullAddress = string.Join(", ",
            new[] { venueAddress, venueCity }.Where(s => !string.IsNullOrWhiteSpace(s)));

        string bookingRef = booking.UniqueCode;
        string ticketCode = ticket.TicketCode;

        decimal unitPrice = booking.PricePerTicket;
        string bookedBy = booking.User?.Name ?? "";
        string bookedEmail = booking.User?.Email ?? "";

        // ── Color Palette — Deep Navy + Gold ────────────────────────────────
        string navy = "#1A1F36";
        string navyLight = "#252B4A";
        string gold = "#F5A623";
        string white = "#FFFFFF";
        string softGrey = "#E2E5F0";
        string midGrey = "#8B93B0";
        string stubBg = "#12172B";

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(780, 290, Unit.Point);
                page.Margin(0);
                page.DefaultTextStyle(x =>
                    x.FontFamily("Helvetica").FontSize(10).FontColor(white));

                page.Content().Background(navy).Row(mainRow =>
                {
                    // ── Left gold accent bar ─────────────────────────────
                    mainRow.ConstantItem(6).Background(gold);

                    // ── Main body ────────────────────────────────────────
                    mainRow.RelativeItem().Padding(28).Column(col =>
                    {
                        // Top row: category badge + brand
                        col.Item().PaddingBottom(12).Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(category))
                            {
                                row.AutoItem()
                                    .Background(gold)
                                    .PaddingHorizontal(10).PaddingVertical(3)
                                    .Text(category.ToUpper())
                                    .FontSize(8).Bold().FontColor(navy);
                            }
                            row.RelativeItem();
                            row.AutoItem()
                                .Text("EVENTPULSE")
                                .FontSize(8).Bold().FontColor(midGrey)
                                .LetterSpacing(0.12f);
                        });

                        // Event title
                        col.Item().PaddingBottom(6)
                            .Text(eventTitle)
                            .FontSize(22).Bold().FontColor(white);

                        // Gold underline
                        col.Item().PaddingBottom(14)
                            .Height(2).Background(gold);

                        // Info grid
                        col.Item().PaddingBottom(10).Row(infoRow =>
                        {
                            // Date block
                            infoRow.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DATE & TIME")
                                    .FontSize(7).Bold().FontColor(gold)
                                    .LetterSpacing(0.1f);
                                c.Item().PaddingTop(3)
                                    .Text(eventDate)
                                    .FontSize(10).FontColor(white);
                                c.Item().PaddingTop(1)
                                    .Text(eventTime)
                                    .FontSize(13).Bold().FontColor(white);
                            });

                            infoRow.ConstantItem(1).PaddingVertical(2).Background(midGrey);

                            // Venue block
                            infoRow.RelativeItem().PaddingLeft(16).Column(c =>
                            {
                                c.Item().Text("VENUE")
                                    .FontSize(7).Bold().FontColor(gold)
                                    .LetterSpacing(0.1f);
                                c.Item().PaddingTop(3)
                                    .Text(venueName)
                                    .FontSize(10).Bold().FontColor(white);
                                c.Item().PaddingTop(1)
                                    .Text(fullAddress)
                                    .FontSize(9).FontColor(midGrey);
                            });

                            // Genre block
                            if (!string.IsNullOrWhiteSpace(genre))
                            {
                                infoRow.ConstantItem(1).PaddingVertical(2).Background(midGrey);
                                infoRow.RelativeItem().PaddingLeft(16).Column(c =>
                                {
                                    c.Item().Text("GENRE")
                                        .FontSize(7).Bold().FontColor(gold)
                                        .LetterSpacing(0.1f);
                                    c.Item().PaddingTop(3)
                                        .Text(genre)
                                        .FontSize(10).FontColor(white);
                                });
                            }
                        });

                        // Footer bar: booking ref + attendee + price
                        col.Item().PaddingTop(6)
                            .Background(navyLight)
                            .Padding(10)
                            .Row(footRow =>
                            {
                                footRow.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("BOOKING REF")
                                        .FontSize(7).Bold().FontColor(gold)
                                        .LetterSpacing(0.08f);
                                    c.Item().PaddingTop(2)
                                        .Text(bookingRef)
                                        .FontSize(9).FontColor(softGrey);
                                });

                                if (!string.IsNullOrWhiteSpace(bookedBy))
                                {
                                    footRow.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("ATTENDEE")
                                            .FontSize(7).Bold().FontColor(gold)
                                            .LetterSpacing(0.08f);
                                        c.Item().PaddingTop(2)
                                            .Text(bookedBy)
                                            .FontSize(9).FontColor(softGrey);
                                        if (!string.IsNullOrWhiteSpace(bookedEmail))
                                            c.Item().Text(bookedEmail)
                                                .FontSize(8).FontColor(midGrey);
                                    });
                                }

                                footRow.AutoItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("TICKET PRICE")
                                        .FontSize(7).Bold().FontColor(gold)
                                        .LetterSpacing(0.08f);
                                    c.Item().PaddingTop(2)
                                        .Text($"Rs. {unitPrice:N2}")
                                        .FontSize(16).Bold().FontColor(white);
                                });
                            });
                    });

                    // ── Right stub ───────────────────────────────────────
                    mainRow.ConstantItem(190).Background(stubBg)
                        .Padding(20).Column(col =>
                        {
                            col.Item().AlignCenter().PaddingBottom(10)
                                .Text("ADMIT ONE")
                                .FontSize(8).Bold().FontColor(gold)
                                .LetterSpacing(0.15f);

                            col.Item().AlignCenter().PaddingBottom(10)
                                .Background(white)
                                .Padding(6)
                                .Width(110).Height(110)
                                .Image(qrBytes).FitArea();

                            col.Item().AlignCenter().PaddingBottom(3)
                                .Text("TICKET CODE")
                                .FontSize(7).Bold().FontColor(gold)
                                .LetterSpacing(0.1f);

                            col.Item().AlignCenter().PaddingBottom(10)
                                .Text(ticketCode)
                                .Bold().FontSize(10).FontColor(white)
                                .LineHeight(1.4f);

                        });
                });
            });
        }).GeneratePdf();
    }
}