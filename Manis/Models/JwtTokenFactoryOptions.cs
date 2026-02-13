namespace Manis.Models;

public sealed class JwtTokenFactoryOptions
{
    public string Key { get; set; } = string.Empty;
    public ushort ExpiresDays { get; set; }
    public ushort RefreshExpiresDays { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
