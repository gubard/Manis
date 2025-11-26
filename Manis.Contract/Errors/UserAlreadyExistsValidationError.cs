using Gaia.Errors;

namespace Manis.Contract.Errors;

public sealed class UserAlreadyExistsValidationError : IdentityValidationError
{
    public UserAlreadyExistsValidationError(string identity) : base(identity)
    {
    }
}