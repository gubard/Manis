using Gaia.Errors;

namespace Manis.Contract.Models;

public class ManisPostRequest
{
    public CreateUser[] CreateUsers { get; set; } = [];
    public ValidationError[] ValidationErrors { get; set; } = [];
}