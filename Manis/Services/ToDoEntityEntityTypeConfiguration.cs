using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manis.Services;

public sealed class UserEntityEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Login).HasMaxLength(255);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.PasswordHash).HasMaxLength(512);
        builder.Property(e => e.PasswordSalt).HasMaxLength(128);
    }
}
