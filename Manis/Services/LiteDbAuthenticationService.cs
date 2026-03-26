using System.Runtime.CompilerServices;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Manis.Contract.Errors;
using Manis.Contract.Models;
using Manis.Contract.Services;
using Manis.Models;
using Nestor.Db.LiteDb.Services;
using Nestor.Db.Models;
using Zeus.Models;

namespace Manis.Services;

public sealed class LiteDbAuthenticationService : IAuthenticationService
{
    public LiteDbAuthenticationService(
        IDatabaseFactory factory,
        ITokenFactory tokenFactory,
        IFactory<string, IHashService<string, string>> hashServiceFactory,
        IAuthenticationValidator authenticationValidator,
        IFactory<DbServiceOptions> factoryOptions
    )
    {
        _factory = factory;
        _tokenFactory = tokenFactory;
        _hashServiceFactory = hashServiceFactory;
        _authenticationValidator = authenticationValidator;
        _factoryOptions = factoryOptions;
    }

    public ConfiguredValueTaskAwaitable<ManisGetResponse> GetAsync(
        ManisGetRequest request,
        CancellationToken ct
    )
    {
        using var database = _factory.Create();
        var collection = database.GetUserEntityCollection();
        var users = collection.FindAll().Select(x => x.ToUserEntity()).ToArray();
        var response = CreateResponse(request, users);

        return TaskHelper.FromResult(response);
    }

    public ConfiguredValueTaskAwaitable<ManisPostResponse> PostAsync(
        Guid idempotentId,
        ManisPostRequest request,
        CancellationToken ct
    )
    {
        var result = new ManisPostResponse();
        using var database = _factory.Create();
        var collection = database.GetUserEntityCollection();
        var options = _factoryOptions.Create();
        var users = collection.FindAll().Select(x => x.ToUserEntity()).ToArray();

        foreach (var createUser in request.CreateUsers)
        {
            var errors = ValidateCreateUser(createUser);
            result.ValidationErrors.AddRange(errors);

            if (errors.Any())
            {
                continue;
            }

            var userByEmail = users.SingleOrDefault(x => x.Email == createUser.Email);

            if (userByEmail is not null)
            {
                result.ValidationErrors.Add(new AlreadyExistsValidationError(createUser.Email));

                continue;
            }

            var userByLogin = users.SingleOrDefault(x => x.Login == createUser.Login);

            if (userByLogin is not null)
            {
                result.ValidationErrors.Add(new AlreadyExistsValidationError(createUser.Login));

                continue;
            }

            if (request.CreateUsers.Count(x => x.Email == createUser.Email) > 1)
            {
                if (
                    !result.ValidationErrors.Any(x =>
                        x is DuplicationValidationError error && error.Identity == createUser.Email
                    )
                )
                {
                    result.ValidationErrors.Add(new DuplicationValidationError(createUser.Email));
                }

                continue;
            }

            if (request.CreateUsers.Count(x => x.Login == createUser.Login) > 1)
            {
                if (
                    !result.ValidationErrors.Any(x =>
                        x is DuplicationValidationError error && error.Identity == createUser.Login
                    )
                )
                {
                    result.ValidationErrors.Add(new DuplicationValidationError(createUser.Login));
                }

                continue;
            }

            var id = Guid.NewGuid();
            var salt = Guid.NewGuid().ToString();

            database.AddEntities(
                id.ToString(),
                idempotentId,
                options.IsUseEvents,
                [
                    new()
                    {
                        Login = createUser.Login,
                        Email = createUser.Email,
                        NormalizeEmail = createUser.Email.ToUpperInvariant(),
                        NormalizeLogin = createUser.Login.ToUpperInvariant(),
                        PasswordSalt = salt,
                        PasswordHash = _hashServiceFactory
                            .Create(NameHelper.Utf8Sha512Hex)
                            .ComputeHash($"{salt};{createUser.Password}"),
                        PasswordHashMethod = NameHelper.Utf8Sha512Hex,
                        Id = id,
                    },
                ]
            );
        }

        database.SaveChanges();

        return TaskHelper.FromResult(result);
    }

    private readonly IDatabaseFactory _factory;
    private readonly ITokenFactory _tokenFactory;
    private readonly IFactory<string, IHashService<string, string>> _hashServiceFactory;
    private readonly IAuthenticationValidator _authenticationValidator;
    private readonly IFactory<DbServiceOptions> _factoryOptions;

    private ValidationError[] ValidateCreateUser(CreateUser createUser)
    {
        var result = new List<ValidationError>();
        result.AddRange(
            _authenticationValidator.Validate(createUser.Email, nameof(createUser.Email))
        );
        result.AddRange(
            _authenticationValidator.Validate(createUser.Login, nameof(createUser.Login))
        );
        result.AddRange(
            _authenticationValidator.Validate(createUser.Password, nameof(createUser.Password))
        );

        return result.ToArray();
    }

    private ManisGetResponse CreateResponse(ManisGetRequest request, UserEntity[] users)
    {
        var result = new ManisGetResponse();

        foreach (var (identity, password) in request.SignIns)
        {
            var user = users.SingleOrDefault(x => x.Login == identity || x.Email == identity);

            if (user is null)
            {
                result.ValidationErrors.Add(new NotFoundValidationError(identity));

                continue;
            }

            var hashService = _hashServiceFactory.Create(user.PasswordHashMethod);

            if (hashService.ComputeHash($"{user.PasswordSalt};{password}") == user.PasswordHash)
            {
                result.SignIns.Add(
                    user.Login,
                    _tokenFactory.Create(
                        new()
                        {
                            Email = user.Email,
                            Login = user.Login,
                            Id = user.Id,
                            Role = Role.User,
                        }
                    )
                );
            }
            else
            {
                result.ValidationErrors.Add(new InvalidPasswordValidationError(identity));
            }
        }

        return result;
    }
}
