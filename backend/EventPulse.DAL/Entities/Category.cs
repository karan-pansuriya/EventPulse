using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities
{
    [Table("categories")]
    public class Category : ActivatableEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}