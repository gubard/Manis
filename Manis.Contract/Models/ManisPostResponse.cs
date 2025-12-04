using Gaia.Models;
using Gaia.Services;

namespace Manis.Contract.Models;

public class ManisPostResponse : IValidationErrors
{
    public List<ValidationError> ValidationErrors { get; set; } = [];
}