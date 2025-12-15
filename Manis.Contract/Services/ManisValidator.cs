using System.Buffers;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;

namespace Manis.Contract.Services;

public interface IAuthenticationValidator : IValidator<string>;

public class AuthenticationValidator : IAuthenticationValidator
{
    private const string ValidLoginChars =
        StringHelper.UpperLatin + StringHelper.LowerLatin + StringHelper.Number + "+-";

    private static readonly SearchValues<char> ValidLoginValues = SearchValues.Create(
        ValidLoginChars
    );

    public ValidationError[] Validate(string value, string identity)
    {
        switch (identity)
        {
            case "Login":
            {
                if (value.Length > 255)
                {
                    return [new PropertyMaxSizeValidationError("Login", (ulong)value.Length, 255)];
                }

                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("Login")];
                }

                if (value.Length < 3)
                {
                    return [new PropertyMinSizeValidationError("Login", (ulong)value.Length, 3)];
                }

                var index = value.IndexOfAnyExcept(ValidLoginValues);

                if (index >= 0)
                {
                    return
                    [
                        new PropertyContainsInvalidValueValidationError<char>(
                            "Login",
                            value[index],
                            ValidLoginChars.ToCharArray()
                        ),
                    ];
                }

                return [];
            }
            case "Password":
            {
                if (value.Length > 512)
                {
                    return
                    [
                        new PropertyMaxSizeValidationError("Password", (ulong)value.Length, 512),
                    ];
                }

                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("Password")];
                }

                if (value.Length < 8)
                {
                    return [new PropertyMinSizeValidationError("Password", (ulong)value.Length, 5)];
                }

                return [];
            }
            case "Email":
            {
                if (value.Length > 255)
                {
                    return [new PropertyMaxSizeValidationError("Email", (ulong)value.Length, 255)];
                }

                if (value.IsNullOrWhiteSpace())
                {
                    return [new PropertyEmptyValidationError("Email")];
                }

                if (!value.IsEmail())
                {
                    return [new PropertyInvalidValidationError("Email")];
                }

                if (value.Length < 5)
                {
                    return [new PropertyMinSizeValidationError("Email", (ulong)value.Length, 5)];
                }

                return [];
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(identity), identity, null);
        }
    }
}
