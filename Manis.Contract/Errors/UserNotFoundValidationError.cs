using Gaia.Errors;

namespace Manis.Contract.Errors;

public sealed class UserNotFoundValidationError : IdentityValidationError
{
    public UserNotFoundValidationError(string identity) : base(identity)
    {
    }
}