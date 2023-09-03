using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;

namespace Passwork.Client.Services;

public class JwtAuthHandler: DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly NavigationManager _navMan;

    public JwtAuthHandler(TokenService token, NavigationManager navMan)
    {
        _tokenService = token;
        _navMan = navMan;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var jwt = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(jwt) == false)
        {
            //add jwt for request - request - return server response
            request.Headers.Add("Authorization", "Bearer " + jwt.Trim('"'));
        }

        var responseMessage = await base.SendAsync(request, cancellationToken);
        return responseMessage;
    }
}
