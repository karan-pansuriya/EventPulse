using System.ComponentModel.DataAnnotations.Schema;
using EventPulse.Common.Entities;

namespace EventPulse.DAL.Entities
{
    [Table("users")]
    public class User : BaseEntity
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public string? Phone { get; set; }

        public required string PasswordHash { get; set; }

        public required string PasswordSalt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    }
}
