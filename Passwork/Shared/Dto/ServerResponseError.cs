namespace Passwork.Shared.Dto;

/// <summary>
/// Класс ответа с информации об ошибки.
/// </summary>
public class ServerResponseError
{
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string Message { get; set; } = "Ошибка";
}