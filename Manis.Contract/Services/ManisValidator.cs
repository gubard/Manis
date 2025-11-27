using Gaia.Errors;
using Gaia.Helpers;
using Gaia.Services;

namespace Manis.Contract.Services;

public interface IManisValidator : IValidator<string>;

public class ManisValidator : IManisValidator
{
    private const string LoginIdentity = "Login";
    private const string PasswordIdentity = "Password";
    private const string EmailIdentity = "Email";

    public ValidationError[] Validate(string value, string identity)
    {
        switch (identity)
        {
            case LoginIdentity:
            {
                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("Login")];
                }

                if (value.Length > 255)
                {
                    return [new PropertyMaxSizeValidationError("Login", (ulong)value.Length, 255)];
                }

                if (value.Length < 3)
                {
                    return [new PropertyMinSizeValidationError("Login", (ulong)value.Length, 3)];
                }

                var index = value.IndexOfAnyExcept(StringHelper.ValidLoginSearch);

                if (index >= 0)
                {
                    return [new PropertyContainsInvalidValueValidationError<char>("Login", value[index], StringHelper.ValidLoginChar.ToCharArray())];
                }

                return [];
            }
            case PasswordIdentity:
            {
                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("password")];
                }

                if (value.Length > 512)
                {
                    return [new PropertyMaxSizeValidationError("password", (ulong)value.Length, 512)];
                }

                if (value.Length < 8)
                {
                    return [new PropertyMinSizeValidationError("password", (ulong)value.Length, 5)];
                }

                return [];
            }
            case EmailIdentity:
            {
                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("email")];
                }

                if (!value.IsEmail())
                {
                    return [new PropertyInvalidValidationError("email")];
                }

                if (value.Length > 255)
                {
                    return [new PropertyMaxSizeValidationError("email", (ulong)value.Length, 255)];
                }

                if (value.Length < 5)
                {
                    return [new PropertyMinSizeValidationError("email", (ulong)value.Length, 5)];
                }

                return [];
            }
            default: throw new ArgumentOutOfRangeException(nameof(identity), identity, null);
        }
    }
}