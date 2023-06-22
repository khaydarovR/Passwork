namespace Passwork.Server.Entity;

public class TagPassword: BaseEntity
{
    public Guid TagId { get; set; }
    public Tag Tag { get; set; }

    public Guid PasswordId { get; set; }
    public Password Password { get; set; }
}
