namespace Passwork.Server.Domain.Entity;

public class CompanyUsers: BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser  { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
    public RightEnum Right { get; set; }
}
