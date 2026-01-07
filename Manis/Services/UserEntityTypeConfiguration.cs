using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Manis.Services;

public sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata.SetValueComparer(
                new ValueComparer<Guid>((c1, c2) => c1 == c2, c => c.GetHashCode(), c => c)
            );

        builder.Property(e => e.Login).HasMaxLength(255);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.PasswordHashMethod).HasMaxLength(255);
        builder.Property(e => e.PasswordHash).HasMaxLength(512);
        builder.Property(e => e.PasswordSalt).HasMaxLength(128);
        builder.Property(e => e.ActivationCode).HasMaxLength(255);
    }
}
