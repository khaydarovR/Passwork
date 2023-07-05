namespace Passwork.Server.Domain;

public static class ActivityNames
{
    public const string Created = "создан";
    public const string UpdatedPw = "обновлен пароль";
    public const string UpdatedLogin = "обновлен логин";
    public const string ReceivedDetailData = "открыл пароль";
    public const string ReadLoginPw = "прочитал логин и пароль";
    public const string Deleted = "мягкое удаление";
    public const string HardDeleted = "оканчательное удаление";
}
