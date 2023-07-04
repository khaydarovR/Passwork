namespace Passwork.Shared.ViewModels;

public class SafeVm
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public RightEnumVm RightInThisSafe { get; set; }
}
