using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities;

[Table("countries")]
public class Country : BaseEntity
{
    public required string Name { get; set; }
    public virtual ICollection<State> States { get; set; } = new List<State>();
}
