using Gaia.Services;
using Manis.CompiledModels;
using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nestor.Db.Services;

namespace Manis.Services;

public interface IAuthenticationDbContext : INestorDbContext
{
    DbSet<UserEntity> Users { get; }
}

public sealed class ManisDbContext
    : NestorDbContext,
        IStaticFactory<DbContextOptions, NestorDbContext>,
        IAuthenticationDbContext
{
    public ManisDbContext() { }

    public ManisDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseModel(ManisDbContextModel.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
    }

    public static NestorDbContext Create(DbContextOptions input)
    {
        return new ManisDbContext(input);
    }
}

public class ManisDbContextFactory : IDesignTimeDbContextFactory<ManisDbContext>
{
    public ManisDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ManisDbContext>();
        optionsBuilder.UseSqlite("");

        return new(optionsBuilder.Options);
    }
}
