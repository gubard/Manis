namespace Manis.Models;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordHashMethod { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActivated { get; set; }
    public string ActivationCode { get; set; } = string.Empty;
}
