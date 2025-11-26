using Nestor.Db;

namespace Manis.Models;

[SourceEntity(nameof(Id))]
public partial class UserEntity
{
    public Guid Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordHashMethod { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
}