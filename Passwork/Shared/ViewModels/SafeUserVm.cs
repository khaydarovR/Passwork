namespace Passwork.Shared.ViewModels;

public class SafeUserVm
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public RightEnum Right { get; set; }
    public string RightDescription { get; set; } = null!;

}