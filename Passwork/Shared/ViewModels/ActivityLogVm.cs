using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.ViewModels;

public class ActivityLogVm
{
    public DateTime At { get; set; }
    public string Title { get; set; }
    public Guid PasswordId { get; set; }
    public Guid AppUsreId { get; set; }
}
