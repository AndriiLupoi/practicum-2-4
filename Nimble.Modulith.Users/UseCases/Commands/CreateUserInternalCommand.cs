using Ardalis.Result;
using Mediator;

namespace Nimble.Modulith.Users.UseCases.Commands;

public record CreateUserInternalCommand(string Email, string Password) : ICommand<Result<string>>;