namespace Passwork.Shared.ViewModels;

public class CompaniesVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public IList<SafeVm> SafeVms { get; set; }
}
