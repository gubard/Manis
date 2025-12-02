using Manis.Contract.Models;

namespace Manis.Contract.Services;

public interface IAuthenticationService
{
    ValueTask<ManisGetResponse> GetAsync(ManisGetRequest request, CancellationToken ct);
    ValueTask<ManisPostResponse> PostAsync(ManisPostRequest request, CancellationToken ct);
}