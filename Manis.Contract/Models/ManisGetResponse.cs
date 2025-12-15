using Gaia.Models;
using Gaia.Services;

namespace Manis.Contract.Models;

public class ManisGetResponse : IValidationErrors
{
    public Dictionary<string, TokenResult> SignIns { get; set; } = [];
    public List<ValidationError> ValidationErrors { get; set; } = [];
}
