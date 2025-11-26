using Gaia.Errors;
using Zeus.Models;

namespace Manis.Contract.Models;

public class ManisGetResponse
{
    public Dictionary<string, TokenResult> SignIns { get; set; } = [];
    public ValidationError[] ValidationErrors { get; set; } = [];
}