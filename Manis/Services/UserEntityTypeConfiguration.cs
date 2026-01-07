using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nestor.Db.Helpers;

namespace Manis.Services;

public sealed class UserEntityTypeConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().SetComparerStruct();
        builder.Property(e => e.Login).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.Email).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.PasswordHashMethod).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.PasswordHash).HasMaxLength(512).SetComparerClass();
        builder.Property(e => e.PasswordSalt).HasMaxLength(128).SetComparerClass();
        builder.Property(e => e.ActivationCode).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.IsActivated).SetComparerStruct();
    }
}
