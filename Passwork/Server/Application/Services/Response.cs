namespace Passwork.Server.Application.Services;

public class Response<T> where T : class
{
    public bool IsSuccessful { get; set; }

    public T ResponseModel { get; set; }

    public List<string> Errors { get; set; } = new List<string>();

    public Response(T responseModel, bool isSuccessful = true) : this(isSuccessful)
    {
        this.ResponseModel = responseModel;
    }

    public Response(string error, bool isSuccessful = false) : this(isSuccessful)
    {
        this.Errors = new List<string>() { error };
    }

    public Response(bool isSuccessful)
    {
        this.IsSuccessful = isSuccessful;
    }
}