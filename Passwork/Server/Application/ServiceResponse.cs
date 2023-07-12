using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.IdentityModel.Tokens;
using Passwork.Shared.ViewModels;

namespace Passwork.Server.Application;

public class ServiceResponse<T>
{
    public bool IsSuccessful
    {
        get
        {
            if (ErrorMessage.Message.IsNullOrEmpty())
            {
                return true;
            }
            return false;
        }
        private set
        {

        }
    }

    public T? ResponseModel { get; set; }

    public ErrorMessage ErrorMessage { get; set; } = new();

    public ServiceResponse()
    {
        
    }

    public ServiceResponse(T responseModel, bool isSuccessful = true) : this(isSuccessful)
    {
        ResponseModel = responseModel;
    }

    public ServiceResponse(string error, bool isSuccessful = false) : this(isSuccessful)
    {
        ErrorMessage.Message = error;
    }

    public ServiceResponse(bool isSuccessful)
    {
        IsSuccessful = isSuccessful;
    }
}