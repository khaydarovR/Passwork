using Microsoft.EntityFrameworkCore.Diagnostics;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.ViewModels;
using System.Runtime.CompilerServices;

namespace Passwork.Server.Utils;

public static class MappingExtensions
{
    public static SafeVm MapToVm(this Safe self)
    {
        return new()
        {
            Id = self.Id,
            Title = self.Title
        };
    }

    public static CompaniesOwnerVm MapToVm(this Company self)
    {
        var res = new CompaniesOwnerVm();
        res.Id = self.Id;
        res.Name = self.Name;

        var safs = new List<SafeVm>();
        foreach (var s in self.Safes)
        {
            safs.Add(s.MapToVm());
        }
        res.SafeVms = safs;
        return res;
    }
}
