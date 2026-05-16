using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Nimble.Modulith.Email.Contracts;
using Nimble.Modulith.Users.Infrastructure;

namespace Nimble.Modulith.Users.Endpoints;

public class ResetPasswordRequest { public string Email { get; set; } = string.Empty; }
public class ResetPasswordResponse { public string Message { get; set; } = string.Empty; public bool Success { get; set; } }

public class ResetPassword(UserManager<IdentityUser> userManager, IMediator mediator)
    : Endpoint<ResetPasswordRequest, ResetPasswordResponse>
{
    public override void Configure()
    {
        Post("/users/reset-password");
        AllowAnonymous();
        Summary(s => { s.Summary = "Reset user password"; s.Description = "Generates a new password and emails it to the user"; });
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);

        if (user == null)
        {
            // Не розкривати чи існує юзер
            Response = new ResetPasswordResponse { Success = true, Message = "If the email exists in our system, a password reset email has been sent." };
            return;
        }

        var newPassword = PasswordGenerator.GeneratePassword();

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded) { AddError("Failed to reset password"); await Send.ErrorsAsync(cancellation: ct); return; }

        var addResult = await userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded) { foreach (var e in addResult.Errors) AddError(e.Description); await Send.ErrorsAsync(cancellation: ct); return; }

        var emailBody = $"Hello,\n\nYour new temporary password is: {newPassword}\n\nPlease log in and change it immediately.\n\nBest regards,\nThe Team";
        await mediator.Send(new SendEmailCommand(user.Email!, "Password Reset - New Temporary Password", emailBody), ct);

        Response = new ResetPasswordResponse { Success = true, Message = "If the email exists in our system, a password reset email has been sent." };
    }
}