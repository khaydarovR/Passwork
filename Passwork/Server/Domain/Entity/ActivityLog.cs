namespace Passwork.Server.Domain.Entity;

public class ActivityLog: BaseEntity
{
    public DateTime At { get; set; }
    public string Title { get; set; }
    public Guid PasswordId { get; set; }
    public Password Password { get; set; }
    public Guid AppUsreId { get; set; }
    public AppUser AppUser { get; set; }
}
