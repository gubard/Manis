using Gaia.Models;

namespace Manis.Contract.Errors;

public sealed class InvalidPasswordValidationError : IdentityValidationError
{
    public InvalidPasswordValidationError(string identity)
        : base(identity) { }
}
