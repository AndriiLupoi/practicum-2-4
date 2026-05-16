using System.Security.Claims;

namespace Nimble.Modulith.Customers.Infrastructure;

public interface ICustomerAuthorizationService
{
    bool IsAdminOrOwner(ClaimsPrincipal user, string customerEmail);
}

public class CustomerAuthorizationService : ICustomerAuthorizationService
{
    public bool IsAdminOrOwner(ClaimsPrincipal user, string customerEmail)
    {
        if (user.IsInRole("Admin"))
            return true;

        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value
                     ?? user.Identity?.Name
                     ?? string.Empty;

        return string.Equals(userEmail, customerEmail, StringComparison.OrdinalIgnoreCase);
    }
}