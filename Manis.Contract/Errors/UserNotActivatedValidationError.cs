using Gaia.Models;

namespace Manis.Contract.Errors;

public sealed class UserNotActivatedValidationError : IdentityValidationError
{
    public UserNotActivatedValidationError(string identity) : base(identity)
    {
    }
}