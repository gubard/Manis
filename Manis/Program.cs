using System.Collections.Frozen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Gaia.Helpers;
using Gaia.Services;
using Manis.Contract.Models;
using Manis.Contract.Services;
using Manis.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Sqlite;
using Zeus.Helpers;
using Zeus.Models;
using Zeus.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<IManisService, ManisService>();
builder.Services.AddTransient<IManisValidator, ManisValidator>();
builder.Services.AddTransient<ITokenFactory, JwtTokenFactory>();
builder.Services.AddTransient<JwtSecurityTokenHandler>();
builder.Services.AddTransient<SHA512>(_ => SHA512.Create());
builder.Services.AddTransient<Sha512HashService>();
builder.Services.AddTransient<StringToUtf8>();
builder.Services.AddTransient<BytesToHex>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolver = ManisJsonContext.Resolver
);
builder.Services.AddTransient<IFactory<string, IHashService<string, string>>>(sp =>
{
    var dic = new Dictionary<string, IHashService<string, string>>
    {
        {
            NameHelper.Utf8Sha512Hex, new StringHashService(sp.GetRequiredService<Sha512HashService>(), sp.GetRequiredService<StringToUtf8>(),
                sp.GetRequiredService<BytesToHex>())
        },
    };

    return new HashServiceFactory(dic.ToFrozenDictionary());
});
builder.Services.AddTransient<JwtTokenFactoryOptions>(sp => sp.GetConfigurationSection<JwtTokenFactoryOptions>("Jwt"));
builder.Services.AddDbContext<DbContext, SqliteNestorDbContext>(options =>
    options.UseSqlite("Data Source=manis.db", x => x.MigrationsAssembly(typeof(SqliteNestorDbContext).Assembly)));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.MapPost(RouteHelper.Get,
        (ManisGetRequest request, IManisService manisService, CancellationToken ct) =>
            manisService.GetAsync(request, ct))
   .WithName(RouteHelper.GetName);

app.MapPost(RouteHelper.Post,
        (ManisPostRequest request, IManisService manisService, CancellationToken ct) =>
            manisService.PostAsync(request, ct))
   .WithName(RouteHelper.PostName);

app.Run();