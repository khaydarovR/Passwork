using Passwork.Server.Domain.Entity;
using Passwork.Shared.ViewModels;

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

    public static TagVm MapToVm(this Tag self)
    {
        var res = new TagVm();
        res.Id = self.Id;
        res.Title = self.Title;

        return res;
    }

    public static PasswordVm MapToVm(this Password self)
    {
        var res = new PasswordVm();
        res.Id = self.Id;
        res.Title = self.Title;
        res.Tags = new List<TagVm>();

        foreach (var pw in self.PasswordTags)
        {
            res.Tags.Add(pw.Tag.MapToVm());
        }

        return res;
    }

    public static PasswordDetailVm MapToDetailVm(this Password self, string masterPw)
    {
        var res = new PasswordDetailVm();
        res.Id = self.Id;
        res.Title = self.Title;
        res.Login = Encryptor.Decrypt(masterPw ,self.Login);
        res.Pw = Encryptor.Decrypt(masterPw ,self.Pw);
        res.Note = self.Note;
        res.UseInUtl = self.UseInUtl;
        res.IsDeleted = self.IsDeleted;
        res.SafeId = self.SafeId;
        res.Tags = new List<TagVm>();

        foreach (var pw in self.PasswordTags)
        {
            res.Tags.Add(pw.Tag.MapToVm());
        }

        return res;
    }
}
