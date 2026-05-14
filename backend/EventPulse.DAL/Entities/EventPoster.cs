using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities
{
    [Table("event_posters")]
    public class EventPoster: BaseEntity
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public string PosterUrl { get; set; } = string.Empty;

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = null!;
    }
}
