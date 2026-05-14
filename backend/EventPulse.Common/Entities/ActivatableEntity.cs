namespace EventPulse.Common.Entities;

public abstract class ActivatableEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
}
