namespace Passwork.Server.Domain.Entity;

public class SafeUsers : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; }

    public Guid SafeId { get; set; }
    public Safe Safe { get; set; }
    public RightEnum Right { get; set; }
}
