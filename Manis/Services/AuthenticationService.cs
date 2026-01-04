using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Manis.Contract.Errors;
using Manis.Contract.Models;
using Manis.Contract.Services;
using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;
using Zeus.Models;

namespace Manis.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly NestorDbContext _dbContext;
    private readonly ITokenFactory _tokenFactory;

    private readonly IFactory<string, IHashService<string, string>> _hashServiceFactory;

    private readonly IAuthenticationValidator _authenticationValidator;

    public AuthenticationService(
        NestorDbContext dbContext,
        ITokenFactory tokenFactory,
        IFactory<string, IHashService<string, string>> hashServiceFactory,
        IAuthenticationValidator authenticationValidator
    )
    {
        _dbContext = dbContext;
        _tokenFactory = tokenFactory;
        _hashServiceFactory = hashServiceFactory;
        _authenticationValidator = authenticationValidator;
    }

    public async ValueTask<ManisGetResponse> GetAsync(ManisGetRequest request, CancellationToken ct)
    {
        var users = await UserEntity.GetEntitiesAsync(CreateQuery(request), ct);

        return CreateResponse(request, users);
    }

    public async ValueTask<ManisPostResponse> PostAsync(
        ManisPostRequest request,
        CancellationToken ct
    )
    {
        var events = _dbContext.Events;
        var identities = request
            .CreateUsers.Select(x => new[] { x.Email, x.Login })
            .SelectMany(x => x)
            .ToArray();
        var result = new ManisPostResponse();

        var ids = events
            .Where(y =>
                events
                    .GroupBy(x => x.EntityId)
                    .Select(e =>
                        e.Where(x =>
                                x.EntityId == e.Key
                                && (
                                    x.EntityProperty == nameof(UserEntity.Login)
                                    || x.EntityProperty == nameof(UserEntity.Email)
                                )
                                && x.EntityType == nameof(UserEntity)
                            )
                            .Max(x => x.Id)
                    )
                    .Contains(y.Id)
            )
            .Where(x => identities.Contains(x.EntityStringValue))
            .Select(x => x.EntityId)
            .Distinct();

        var users = await UserEntity.GetEntitiesAsync(
            events.Where(x => ids.Contains(x.EntityId)).AsNoTracking(),
            ct
        );

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

            await UserEntity.AddEntitiesAsync(
                _dbContext,
                id.ToString(),
                [
                    new()
                    {
                        Login = createUser.Login,
                        Email = createUser.Email,
                        PasswordSalt = salt,
                        PasswordHash = _hashServiceFactory
                            .Create(NameHelper.Utf8Sha512Hex)
                            .ComputeHash($"{salt};{createUser.Password}"),
                        PasswordHashMethod = NameHelper.Utf8Sha512Hex,
                        Id = id,
                    },
                ],
                ct
            );
        }

        await _dbContext.SaveChangesAsync(ct);

        return result;
    }

    public ManisPostResponse Post(ManisPostRequest request)
    {
        var events = _dbContext.Events;

        var identities = request
            .CreateUsers.Select(x => new[] { x.Email, x.Login })
            .SelectMany(x => x)
            .ToArray();

        var result = new ManisPostResponse();

        var ids = events
            .Where(y =>
                events
                    .GroupBy(x => x.EntityId)
                    .Select(e =>
                        e.Where(x =>
                                x.EntityId == e.Key
                                && (
                                    x.EntityProperty == nameof(UserEntity.Login)
                                    || x.EntityProperty == nameof(UserEntity.Email)
                                )
                                && x.EntityType == nameof(UserEntity)
                            )
                            .Max(x => x.Id)
                    )
                    .Contains(y.Id)
            )
            .Where(x => identities.Contains(x.EntityStringValue))
            .Select(x => x.EntityId)
            .Distinct();

        var users = UserEntity.GetEntities(
            events.Where(x => ids.Contains(x.EntityId)).AsNoTracking()
        );

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

            UserEntity.AddEntities(
                _dbContext,
                id.ToString(),
                [
                    new()
                    {
                        Login = createUser.Login,
                        Email = createUser.Email,
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

        _dbContext.SaveChanges();

        return result;
    }

    public ManisGetResponse Get(ManisGetRequest request)
    {
        var users = UserEntity.GetEntities(CreateQuery(request));

        return CreateResponse(request, users);
    }

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

    private IQueryable<EventEntity> CreateQuery(ManisGetRequest request)
    {
        var events = _dbContext.Events;
        var identities = request.SignIns.Select(x => x.Key).ToArray();

        var ids = events
            .GetProperty(nameof(UserEntity), nameof(UserEntity.Email))
            .Where(x => identities.Contains(x.EntityStringValue))
            .Select(x => x.EntityId)
            .Distinct()
            .Concat(
                events
                    .GetProperty(nameof(UserEntity), nameof(UserEntity.Login))
                    .Where(x => identities.Contains(x.EntityStringValue))
                    .Select(x => x.EntityId)
                    .Distinct()
            );

        return events.Where(x => ids.Contains(x.EntityId));
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
