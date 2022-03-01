using Contracts.Catalog.Request;
using Contracts.Catalog.Response;

namespace Services.Abstractions.Shared
{
    public interface ITokenService
    {
        Task<TokenResponse?> GenerateAccessTokenAsync(LoginUser request, CancellationToken cancellationToken = default);
        Task<TokenResponse?> RenewTokenAsync(Token request, CancellationToken cancellationToken = default);
        Task RevokeTokenAsync(CancellationToken cancellationToken = default);
    }
}
