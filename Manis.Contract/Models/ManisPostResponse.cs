using Gaia.Errors;

namespace Manis.Contract.Models;

public class ManisPostResponse
{
    public ValidationError[] ValidationErrors { get; set; } = [];
}