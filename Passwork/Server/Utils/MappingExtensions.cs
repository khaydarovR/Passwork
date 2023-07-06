using Microsoft.IdentityModel.Tokens;
using Passwork.Server.Domain;
using Passwork.Server.Domain.Entity;
using Passwork.Shared.ViewModels;

namespace Passwork.Server.Utils;

public static class MappingExtensions
{
    public static SafeVm MapToVm(this Safe self, Guid userId)
    {
        if (self.SafeUsers.IsNullOrEmpty())
        {
            throw new NullReferenceException("Для маппинга требуется прикрепить к сейфу их пользователей");
        }
        return new()
        {
            Id = self.Id,
            Title = self.Title,
            RightInThisSafe = self.SafeUsers.Where(su => su.AppUserId == userId).Single(s => (s.SafeId == self.Id)).Right.MapToVm(),
        };
    }

    public static CompaniesVm MapToVm(this Company self, Guid userId)
    {
        var res = new CompaniesVm();
        res.Id = self.Id;
        res.Name = self.Name;

        var safs = new List<SafeVm>();
        foreach (var s in self.Safes)
        {
            if(s.SafeUsers.FirstOrDefault(su => su.AppUserId == userId) == null) { continue; }
            safs.Add(s.MapToVm(userId));
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


    public static SafeUserVm MapToVm(this SafeUsers self)
    {
        var res = new SafeUserVm();

        res.Email = self.AppUser.Email;
        res.UserId = self.AppUserId;
        res.Right = self.Right.MapToVm();

        return res;
    }

    public static RightEnumVm MapToVm(this RightEnum self) =>
    self switch
    {
        RightEnum.None => RightEnumVm.Отсутствует,
        RightEnum.Visible => RightEnumVm.Смотреть,
        RightEnum.Read => RightEnumVm.Читать,
        RightEnum.Write => RightEnumVm.Записывать,
        RightEnum.Invite => RightEnumVm.Приглашать,
        RightEnum.Delete => RightEnumVm.Удалять,
        RightEnum.Owner => RightEnumVm.Владелец,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, "Неизвестное значение перечисления RightEnum")
    };

    public static RightEnum MapToEnum(this RightEnumVm self) =>
    self switch
    {
        RightEnumVm.Отсутствует => RightEnum.None,
        RightEnumVm.Смотреть => RightEnum.Visible,
        RightEnumVm.Читать => RightEnum.Read,
        RightEnumVm.Записывать => RightEnum.Write,
        RightEnumVm.Приглашать => RightEnum.Invite,
        RightEnumVm.Удалять => RightEnum.Delete,
        RightEnumVm.Владелец => RightEnum.Owner,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, "Неизвестное значение перечисления RightEnumVm")
    };

    public static ComUserVm MapToComUserVm(this AppUser self)
    {
        var res = new ComUserVm();

        res.Email = self.Email;
        res.Id = self.Id;

        return res;
    }

    private static Password MapToPassword(this PasswordDetailVm self, string masterPw)
    {
        var res = new Password();

        res.Title = self.Title;
        res.IsDeleted = self.IsDeleted;
        res.Login = Encryptor.Encrypt(masterPw, self.Login);
        res.Pw = Encryptor.Encrypt(masterPw, self.Pw);
        res.Note = self.Note;
        res.UseInUtl = self.UseInUtl;

        return res;
    }
}
