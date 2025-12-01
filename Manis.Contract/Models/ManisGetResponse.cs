using Gaia.Errors;
using Gaia.Services;
using Zeus.Models;

namespace Manis.Contract.Models;

public class ManisGetResponse : IValidationErrors
{
    public Dictionary<string, TokenResult> SignIns { get; set; } = [];
    public ValidationError[] ValidationErrors { get; set; } = [];
}