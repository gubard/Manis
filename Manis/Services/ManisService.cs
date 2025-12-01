using Gaia.Errors;
using Gaia.Helpers;
using Gaia.Services;
using Manis.Contract.Errors;
using Manis.Contract.Models;
using Manis.Contract.Services;
using Manis.Models;
using Microsoft.EntityFrameworkCore;
using Nestor.Db;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Zeus.Models;
using Zeus.Services;

namespace Manis.Services;

public class ManisService : IManisService
{
    private readonly DbContext _dbContext;
    private readonly ITokenFactory _tokenFactory;
    private readonly IFactory<string, IHashService<string, string>> _hashServiceFactory;
    private readonly IManisValidator _manisValidator;
    private static readonly string[] Identities = [nameof(UserEntity.Login), nameof(UserEntity.Email)];

    public ManisService(DbContext dbContext, ITokenFactory tokenFactory,
        IFactory<string, IHashService<string, string>> hashServiceFactory, IManisValidator manisValidator)
    {
        _dbContext = dbContext;
        _tokenFactory = tokenFactory;
        _hashServiceFactory = hashServiceFactory;
        _manisValidator = manisValidator;
    }

    public async ValueTask<ManisGetResponse> GetAsync(ManisGetRequest request, CancellationToken ct)
    {
        var validationErrors = new List<ValidationError>();
        var events = _dbContext.Set<EventEntity>();
        var identities = request.SignIns.Select(x => x.Key).ToArray();
        var result = new ManisGetResponse();

        var ids = events.GetProperty(nameof(UserEntity), nameof(UserEntity.Email))
           .Where(x => identities.Contains(x.EntityStringValue))
           .Select(x => x.EntityId)
           .Distinct()
           .Concat(events.GetProperty(nameof(UserEntity), nameof(UserEntity.Login)).Where(x => identities.Contains(x.EntityStringValue))
               .Select(x => x.EntityId)
               .Distinct());

        var users = await UserEntity.GetUserEntitysAsync(events.Where(x => ids.Contains(x.EntityId)), ct);

        foreach (var (identity, password) in request.SignIns)
        {
            var user = users.SingleOrDefault(x => x.Login == identity || x.Email == identity);

            if (user is null)
            {
                validationErrors.Add(new NotFoundValidationError(identity));

                continue;
            }

            /*if (!user.IsActivated)
            {
                validationErrors.Add(new UserNotActivatedValidationError(identity));

                continue;
            }*/

            var hashService = _hashServiceFactory.Create(user.PasswordHashMethod);

            if (hashService.ComputeHash($"{user.PasswordSalt};{password}") == user.PasswordHash)
            {
                result.SignIns.Add(user.Login, _tokenFactory.Create(new()
                {
                    Email = user.Email,
                    Login = user.Login,
                    Id = user.Id,
                    Role = Role.User,
                }));
            }
            else
            {
                validationErrors.Add(new InvalidPasswordValidationError(identity));
            }
        }

        result.ValidationErrors = validationErrors.ToArray();

        return result;
    }

    public async ValueTask<ManisPostResponse> PostAsync(ManisPostRequest request, CancellationToken ct)
    {
        var validationErrors = new List<ValidationError>();
        var events = _dbContext.Set<EventEntity>();
        var identities = request.CreateUsers.Select(x => new[]
        {
            x.Email,
            x.Login,
        }).SelectMany(x => x).ToArray();
        var result = new ManisPostResponse();

        var ids = events.Where(y => events.GroupBy(x => x.EntityId)
               .Select(e =>
                    e.Where(x =>
                            x.EntityId == e.Key
                         && (x.EntityProperty == nameof(UserEntity.Login) ||
                                x.EntityProperty == nameof(UserEntity.Email))
                         && x.EntityType == nameof(UserEntity))
                       .Max(x => x.Id))
               .Contains(y.Id))
           .Where(x => identities.Contains(x.EntityStringValue))
           .Select(x => x.EntityId)
           .Distinct();

        var users = await UserEntity.GetUserEntitysAsync(events.Where(x => ids.Contains(x.EntityId)), ct);

        foreach (var createUser in request.CreateUsers)
        {
            var errors = ValidateCreateUser(createUser);
            validationErrors.AddRange(errors);

            if (errors.Any())
            {
                continue;
            }

            var userByEmail = users.SingleOrDefault(x => x.Email == createUser.Email);

            if (userByEmail is not null)
            {
                validationErrors.Add(new AlreadyExistsValidationError(createUser.Email));

                continue;
            }

            var userByLogin = users.SingleOrDefault(x => x.Login == createUser.Login);

            if (userByLogin is not null)
            {
                validationErrors.Add(new AlreadyExistsValidationError(createUser.Login));

                continue;
            }

            if (request.CreateUsers.Count(x => x.Email == createUser.Email) > 1)
            {
                if (!validationErrors.Any(x => x is DuplicationValidationError error && error.Identity == createUser.Email))
                {
                    validationErrors.Add(new DuplicationValidationError(createUser.Email));
                }


                continue;
            }

            if (request.CreateUsers.Count(x => x.Login == createUser.Login) > 1)
            {
                if (!validationErrors.Any(x => x is DuplicationValidationError error && error.Identity == createUser.Login))
                {
                    validationErrors.Add(new DuplicationValidationError(createUser.Login));
                }


                continue;
            }

            var id = Guid.NewGuid();
            var salt = Guid.NewGuid().ToString();

            await UserEntity.AddUserEntitysAsync(_dbContext, id.ToString(), ct, [
                new()
                {
                    Login = createUser.Login,
                    Email = createUser.Email,
                    PasswordSalt = salt,
                    PasswordHash = _hashServiceFactory.Create(NameHelper.Utf8Sha512Hex).ComputeHash($"{salt};{createUser.Password}"),
                    PasswordHashMethod = NameHelper.Utf8Sha512Hex,
                    Id = id,
                },
            ]);
        }

        result.ValidationErrors = validationErrors.ToArray();
        await _dbContext.SaveChangesAsync(ct);

        return result;
    }

    private ValidationError[] ValidateCreateUser(CreateUser createUser)
    {
        var result = new List<ValidationError>();
        result.AddRange(_manisValidator.Validate(createUser.Email, nameof(createUser.Email)));
        result.AddRange(_manisValidator.Validate(createUser.Login, nameof(createUser.Login)));
        result.AddRange(_manisValidator.Validate(createUser.Password, nameof(createUser.Password)));

        return result.ToArray();
    }
}