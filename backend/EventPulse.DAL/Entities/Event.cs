using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities
{
    [Table("events")]
    public class Event : ActivatableEntity
    {
        [Required]
        public int OrganizerId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        [MaxLength(10)]
        public string? AgeRestriction { get; set; }

        public string? Performers { get; set; }

        public int? DurationMins { get; set; }

        [Required]
        [Column("event_date", TypeName = "date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Column("start_time", TypeName = "time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        public bool IsVerified { get; set; } = false;

        public int? VenueId { get; set; }

        [ForeignKey(nameof(OrganizerId))]
        public virtual User Organizer { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }

        [ForeignKey(nameof(VenueId))]
        public virtual Venue? Venue { get; set; }

        public virtual ICollection<EventPoster> Posters { get; set; } = new List<EventPoster>();

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
