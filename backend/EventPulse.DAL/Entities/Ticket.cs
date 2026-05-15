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

        public string? QrCode { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; } = null!;
    }
}
