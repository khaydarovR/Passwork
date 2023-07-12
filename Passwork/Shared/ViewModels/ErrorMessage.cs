namespace Passwork.Shared.ViewModels;

public class ErrorMessage
{
    public string? Message { get; set; } = null;
    public string? Description { get; set; }

    public ErrorMessage(string errorMessage)
    {
        Message = errorMessage;
    }
    public ErrorMessage()
    {
        
    }
}
