using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities;

[Table("states")]
public class State : BaseEntity
{
    public required string Name { get; set; }
    public int CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
