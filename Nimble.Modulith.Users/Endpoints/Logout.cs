using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Nimble.Modulith.Users.Endpoints;

public class LogoutResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class Logout(SignInManager<IdentityUser> signInManager) : EndpointWithoutRequest<LogoutResponse>
{
    public override void Configure()
    {
        Post("/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await signInManager.SignOutAsync();
        Response = new LogoutResponse { Success = true, Message = "Logged out successfully" };
        await Send.OkAsync(Response, ct);
    }
}