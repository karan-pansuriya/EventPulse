using System.ComponentModel.DataAnnotations.Schema;

namespace EventPulse.DAL.Entities
{
    [Table("user_roles")]
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;
    }
}
