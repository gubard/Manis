using System.Collections.Frozen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Gaia.Helpers;
using Gaia.Services;
using Manis.Contract.Models;
using Manis.Contract.Services;
using Manis.Helpers;
using Manis.Models;
using Manis.Services;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;
using Zeus.Helpers;

var migration = new Dictionary<int, string>();

foreach (var (key, value) in SqliteMigration.Migrations)
{
    migration.Add(key, value);
}

foreach (var (key, value) in ManisMigration.Migrations)
{
    migration.Add(key, value);
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IAuthenticationValidator, AuthenticationValidator>();
builder.Services.AddTransient<ITokenFactory, JwtTokenFactory>();
builder.Services.AddTransient<IMigrator>(_ => new Migrator(migration.ToFrozenDictionary()));
builder.Services.AddTransient<JwtSecurityTokenHandler>();
builder.Services.AddTransient<SHA512>(_ => SHA512.Create());
builder.Services.AddTransient<Sha512HashService>();
builder.Services.AddTransient<StringToUtf8>();
builder.Services.AddTransient<BytesToHex>();
builder.Services.AddTransient<IStorageService>(_ => new StorageService("Manis"));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolver = ManisJsonContext.Resolver
);

builder.Services.AddTransient<IFactory<string, IHashService<string, string>>>(sp =>
{
    var dic = new Dictionary<string, IHashService<string, string>>
    {
        {
            NameHelper.Utf8Sha512Hex,
            new StringHashService(
                sp.GetRequiredService<Sha512HashService>(),
                sp.GetRequiredService<StringToUtf8>(),
                sp.GetRequiredService<BytesToHex>()
            )
        },
    };

    return new HashServiceFactory(dic.ToFrozenDictionary());
});

builder.Services.AddTransient<JwtTokenFactoryOptions>(sp =>
    sp.GetConfigurationSection<JwtTokenFactoryOptions>("Jwt")
);

builder.Services.AddScoped<IDbConnectionFactory>(sp =>
{
    var storageService = sp.GetRequiredService<IStorageService>();
    var file = storageService.GetDbDirectory().ToFile("manis.db");

    return new SqliteDbConnectionFactory(new() { DataSource = $"Data Source={file}" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost(
        RouteHelper.Get,
        async (
            ManisGetRequest request,
            IAuthenticationService authenticationService,
            CancellationToken ct
        ) => await authenticationService.GetAsync(request, ct)
    )
    .WithName(RouteHelper.GetName);

app.MapPost(
        RouteHelper.Post,
        async (
            ManisPostRequest request,
            IAuthenticationService authenticationService,
            IHttpContextAccessor accessor,
            CancellationToken ct
        ) =>
            await authenticationService.PostAsync(
                accessor.HttpContext.ThrowIfNull().GetIdempotentId(),
                request,
                ct
            )
    )
    .WithName(RouteHelper.PostName);

app.Services.CreateDbDirectory();
await app.Services.MigrateDbAsync(CancellationToken.None);
await app.RunAsync(CancellationToken.None);
