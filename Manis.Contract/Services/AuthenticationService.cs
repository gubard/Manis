using Gaia.Services;
using Manis.Contract.Models;

namespace Manis.Contract.Services;

public interface IAuthenticationService
    : IService<ManisGetRequest, ManisPostRequest, ManisGetResponse, ManisPostResponse>;
