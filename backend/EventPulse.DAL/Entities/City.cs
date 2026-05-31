using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities;

[Table("cities")]
public class City : BaseEntity
{
    public required string Name { get; set; }
    public int StateId { get; set; }

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    public virtual ICollection<Venue> Venues { get; set; } = new List<Venue>();
}
