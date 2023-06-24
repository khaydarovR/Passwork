using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.ViewModels;

public class CompaniesOwnerVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public IList<SafeVm> SafeVms {get; set; }
}
