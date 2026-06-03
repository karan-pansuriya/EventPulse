using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities
{
    [Table("tickets")]
    public class Ticket : ActivatableEntity
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public string TicketCode { get; set; } = string.Empty;

        public string? QrCodePath { get; set; }

        public string? PdfPath { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedAt { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;
    }
}
