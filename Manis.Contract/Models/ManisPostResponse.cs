using Gaia.Errors;
using Gaia.Services;

namespace Manis.Contract.Models;

public class ManisPostResponse : IValidationErrors
{
    public ValidationError[] ValidationErrors { get; set; } = [];
}