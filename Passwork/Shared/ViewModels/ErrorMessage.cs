namespace Passwork.Shared.ViewModels;

public class ErrorMessage<T>
{
    public string Message { get; set; } = null!;
    public string? Description { get; set; }
    public T? Model { get; set; }
}
