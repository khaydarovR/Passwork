namespace Passwork.Server.Domain.Entity;

public class PasswordTags
{
    public Guid TagId { get; set; }
    public Tag Tag { get; set; }

    public Guid PasswordId { get; set; }
    public Password Password { get; set; }
}
