using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Nimble.Modulith.Users.Events;

namespace Nimble.Modulith.Users.Endpoints;

public class AddRoleToUserRequest { public string RoleName { get; set; } = string.Empty; }
public class AddRoleToUserResponse { public string Message { get; set; } = string.Empty; }

public class AddRoleToUser : Endpoint<AddRoleToUserRequest, AddRoleToUserResponse>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMediator _mediator;

    public AddRoleToUser(UserManager<IdentityUser> userManager, IMediator mediator)
    {
        _userManager = userManager;
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/users/{id}/roles");
        AllowAnonymous(); // пізніше змінимо на Roles("Admin")
    }

    public override async Task HandleAsync(AddRoleToUserRequest req, CancellationToken ct)
    {
        var userId = Route<string>("id")!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) { AddError("User not found"); await Send.ErrorsAsync(cancellation: ct); return; }

        var normalizedRole = char.ToUpper(req.RoleName[0]) + req.RoleName[1..].ToLower();
        if (normalizedRole != "Admin" && normalizedRole != "Customer")
        {
            AddError($"Invalid role. Valid roles: Admin, Customer");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (await _userManager.IsInRoleAsync(user, normalizedRole))
        {
            AddError($"User is already in role '{normalizedRole}'");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!result.Succeeded) { foreach (var e in result.Errors) AddError(e.Description); await Send.ErrorsAsync(cancellation: ct); return; }

        await _mediator.Publish(new UserAddedToRoleEvent(user.Id, user.Email!, normalizedRole), ct);
        Response.Message = $"User '{user.Email}' added to role '{normalizedRole}'";
    }
}