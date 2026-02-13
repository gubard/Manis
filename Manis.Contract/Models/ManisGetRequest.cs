namespace Manis.Contract.Models;

public sealed class ManisGetRequest
{
    public Dictionary<string, string> SignIns { get; set; } = [];
}
