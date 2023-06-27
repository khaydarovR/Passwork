using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passwork.Shared.ViewModels;

public class ErrorMessage<T>
{
    public string Message { get; set; } = null!;
    public string? Description { get; set; }
    public T? Model { get; set; }
}
