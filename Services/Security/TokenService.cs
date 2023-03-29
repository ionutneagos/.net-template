using Contracts.Catalog.Request;
using Contracts.Catalog.Response;
using Domain.Constants;
using Domain.Entities.Catalog;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Abstractions;
using Services.Abstractions.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Shared
{
    public sealed class TokenService : ITokenService
    {
        private readonly IServiceManager serviceManager;
        private readonly UserManager<ApplicationUser> userManager;

        public TokenService(IServiceManager serviceManager, UserManager<ApplicationUser> userManager)
        {
            this.serviceManager = serviceManager;
            this.userManager = userManager;
        }

        public async Task<TokenResponse?> GenerateAccessTokenAsync(LoginUser request, CancellationToken cancellationToken = default)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return null;

            if (!await userManager.CheckPasswordAsync(user, request.Password))
                return null;

            TokenResponse? tokenResponse = await GenerateAccessTokenAsync(user, cancellationToken: cancellationToken);

            if (tokenResponse == null)
                throw new NotFoundException("Invalid token");

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpires = tokenResponse.RefreshTokenExpiresAt;

            await serviceManager.AppUserService.UpdateAsync(user, cancellationToken);

            return tokenResponse;
        }

        public async Task<TokenResponse?> RenewTokenAsync(Token request, CancellationToken cancellationToken = default)
        {
            string accessToken = request.AccessToken;
            string refreshToken = request.RefreshToken;

            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(accessToken);
            string? userId = principal.Identity?.Name;
            if (userId == null)
                return null;

            ApplicationUser user = await serviceManager.AppUserService.GetByIdAsync(userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpires <= DateTime.UtcNow)
                throw new NotFoundException("Invalid token");

            TokenResponse? tokenResponse = await GenerateAccessTokenAsync(user, principal?.Identities.FirstOrDefault(), cancellationToken);

            if (tokenResponse == null)
                throw new NotFoundException("Invalid token");

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpires = tokenResponse.RefreshTokenExpiresAt;
            await serviceManager.AppUserService.UpdateAsync(user, cancellationToken);

            return tokenResponse;
        }

        public async Task RevokeTokenAsync(CancellationToken cancellationToken = default)
        {
            string? userId = serviceManager.User.Identity?.Name;

            if (userId == null)
                throw new NotFoundException("Invalid token");

            ApplicationUser user = await serviceManager.AppUserService.GetByIdAsync(userId);

            user.RefreshToken = string.Empty;
            user.RefreshTokenExpires = null;
            await serviceManager.AppUserService.UpdateAsync(user, cancellationToken);
        }

        #region Private Methods
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceManager.Configuration["Authentication:Jwt:Secret"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidIssuer = serviceManager.Configuration["Authentication:Jwt:Issuer"],
                ValidAudience = serviceManager.Configuration["Authentication:Jwt:Audience"],
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        private async Task<TokenResponse?> GenerateAccessTokenAsync(ApplicationUser user, ClaimsIdentity? claimsIdentity = null, CancellationToken cancellationToken = default)
        {
            _ = int.TryParse(serviceManager.Configuration["Authentication:Jwt:AccessTokenExpiresInMinutes"], out int accessTokenExpiresInMinutes);
            _ = int.TryParse(serviceManager.Configuration["Authentication:Jwt:RefreshTokenExpiresInHours"], out int refreshTokenExpiresInHours);

            DateTime accessTokenExpires = DateTime.Now.AddMinutes(accessTokenExpiresInMinutes);
            ClaimsIdentity userClaims = claimsIdentity ?? await CreateUserClaims(user, accessTokenExpires);

            string token = CreateTokenFromClaims(userClaims, DateTime.UtcNow.AddMinutes(accessTokenExpiresInMinutes));
            string refreshToken = await GenerateRefreshTokenAsync(cancellationToken);
            DateTime refreshTokenExpires = DateTime.Now.AddHours(refreshTokenExpiresInHours);

            return new TokenResponse
            {
                AccessToken = token,
                AccessTokenExpiresAt = accessTokenExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpires,
                Scopes = userClaims.FindAll(x => x.Type == ClaimTypes.Role).Select(t => t.Value).ToList(),
                TokenType = "Bearer"
            };
        }
        private async Task<string> GenerateRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            string refreshToken = await GetUniqueTokenAsync(cancellationToken);

            return refreshToken;

            async Task<string> GetUniqueTokenAsync(CancellationToken cancellationToken)
            {
                // token is a cryptographically strong random sequence of values
                string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                // ensure token is unique by checking against db
                bool tokenIsNotUnique = await serviceManager.AppUserService.GetAll().Where(x => x.RefreshToken == token).AnyAsync(cancellationToken);

                if (tokenIsNotUnique)
                    return await GetUniqueTokenAsync(cancellationToken);

                return token;
            }
        }
        private async Task<ClaimsIdentity> CreateUserClaims(ApplicationUser user, DateTime expires)
        {
            List<string> userRoles = (await userManager.GetRolesAsync(user)).ToList();

            ClaimsIdentity claims = new(new[]
            {
                    new Claim(ClaimTypes.Name, user.Id) ,
                    new Claim(JwtRegisteredClaimNames.Sub,serviceManager.Configuration["Authentication:Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, user.Id),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim(JwtRegisteredClaimNames.Exp, expires.ToString()),
                    new Claim(ClaimTypes.GivenName, user.LastName ?? ""),
                    new Claim(ClaimTypes.Surname, user.FirstName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ContextConfiguration.TenantIdClaim, user.TenantId?.ToString() ?? string.Empty),
                });

            if (userRoles.Any())
            {
                claims.AddClaims(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            return claims;
        }
        private string CreateTokenFromClaims(ClaimsIdentity userClaims, DateTime expires)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = userClaims,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceManager.Configuration["Authentication:Jwt:Secret"])),
                    SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = DateTime.Now,
                TokenType = "Bearer",
                Issuer = serviceManager.Configuration["Authentication:Jwt:Issuer"],
                Audience = serviceManager.Configuration["Authentication:Jwt:Audience"]
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
        #endregion
    }
}
