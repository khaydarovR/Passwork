namespace Passwork.Shared.ViewModels;

public class SafeUserVm
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public RightEnumVm Right { get; set; }

}