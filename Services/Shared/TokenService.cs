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

        public TokenService(UserManager<ApplicationUser> userManager, IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
            this.userManager = userManager;
        }

        public async Task<TokenResponse?> GenerateAccessTokenAsync(LoginUser request, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return null;

            if (!await userManager.CheckPasswordAsync(user, request.Password))
                return null;

            var tokenResponse = await GenerateAccessTokenAsync(user, cancellationToken: cancellationToken);

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

            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.Identity?.Name;
            if (userId == null)
                return null;

            var user = await serviceManager.AppUserService.GetByIdAsync(userId);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpires <= DateTime.UtcNow)
                throw new NotFoundException("Invalid token");

            var tokenResponse = await GenerateAccessTokenAsync(user, principal?.Identities.FirstOrDefault(), cancellationToken);

            if (tokenResponse == null)
                throw new NotFoundException("Invalid token");

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpires = tokenResponse.RefreshTokenExpiresAt;
            await serviceManager.AppUserService.UpdateAsync(user, cancellationToken);

            return tokenResponse;
        }

        public async Task RevokeTokenAsync(CancellationToken cancellationToken = default)
        {
            var userId = serviceManager.User.Identity?.Name;

            if (userId == null)
                throw new NotFoundException("Invalid token");

            var user = await serviceManager.AppUserService.GetByIdAsync(userId);

            user.RefreshToken = string.Empty;
            user.RefreshTokenExpires = null;
            await serviceManager.AppUserService.UpdateAsync(user, cancellationToken);
        }

        #region Private Methods
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceManager.Configuration["Authentication:Jwt:Secret"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidIssuer = serviceManager.Configuration["Authentication:Jwt:Issuer"],
                ValidAudience = serviceManager.Configuration["Authentication:Jwt:Audience"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        private async Task<TokenResponse?> GenerateAccessTokenAsync(ApplicationUser user, ClaimsIdentity? claimsIdentity = null, CancellationToken cancellationToken = default)
        {
            _ = int.TryParse(serviceManager.Configuration["Authentication:Jwt:AccessTokenExpiresInMinutes"], out int accessTokenExpiresInMinutes);
            _ = int.TryParse(serviceManager.Configuration["Authentication:Jwt:RefreshTokenExpiresInHours"], out int refreshTokenExpiresInHours);

            var accessTokenExpires = DateTime.Now.AddMinutes(accessTokenExpiresInMinutes);
            var userClaims = claimsIdentity ?? await CreateUserClaims(user, accessTokenExpires);

            var token = CreateTokenFromClaims(userClaims, DateTime.UtcNow.AddMinutes(accessTokenExpiresInMinutes));
            var refreshToken = await GenerateRefreshTokenAsync(cancellationToken);
            var refreshTokenExpires = DateTime.Now.AddHours(refreshTokenExpiresInHours);

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
            var refreshToken = await GetUniqueTokenAsync(cancellationToken);

            return refreshToken;

            async Task<string> GetUniqueTokenAsync(CancellationToken cancellationToken)
            {
                // token is a cryptographically strong random sequence of values
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                // ensure token is unique by checking against db
                var tokenIsNotUnique = await serviceManager.AppUserService.GetAll().Where(x => x.RefreshToken == token).AnyAsync(cancellationToken);

                if (tokenIsNotUnique)
                    return await GetUniqueTokenAsync(cancellationToken);

                return token;
            }
        }
        private async Task<ClaimsIdentity> CreateUserClaims(ApplicationUser user, DateTime expires)
        {
            var userRoles = (await userManager.GetRolesAsync(user)).ToList();

            var claims = new ClaimsIdentity(new[]
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
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = userClaims,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(serviceManager.Configuration["Authentication:Jwt:Secret"])),
                    SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = DateTime.Now,
                TokenType= "Bearer",
                Issuer = serviceManager.Configuration["Authentication:Jwt:Issuer"],
                Audience = serviceManager.Configuration["Authentication:Jwt:Audience"]
            };
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
        #endregion
    }
}
