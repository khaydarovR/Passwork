using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.ViewModels;

public class CurrentUser
{
    public string Id { get; set; }
    public string Email { get; set; }
    public IEnumerable<Claim> ClaimList { get; set; }
}
