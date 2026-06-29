using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;
using EventPulse.DAL.Enums;

namespace EventPulse.DAL.Entities
{
    [Table("bookings")]
    public class Booking : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public string UniqueCode { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column("price_per_ticket", TypeName = "decimal(10,2)")]
        public decimal PricePerTicket { get; set; }

        [Required]
        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [MaxLength(200)]
        public string? PaymentRef { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = null!;

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
