using Gaia.Models;
using Gaia.Services;

namespace Manis.Contract.Models;

public sealed class ManisPostResponse : IValidationErrors
{
    public List<ValidationError> ValidationErrors { get; set; } = [];
}
