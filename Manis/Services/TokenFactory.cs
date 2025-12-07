using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gaia.Models;
using Gaia.Services;
using Manis.Models;
using Microsoft.IdentityModel.Tokens;
using Zeus.Models;

namespace Manis.Services;

public interface ITokenFactory : IFactory<UserTokenClaims, TokenResult>, IFactory<TokenResult>;

public class JwtTokenFactory : ITokenFactory
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly JwtTokenFactoryOptions _options;

    public JwtTokenFactory(JwtTokenFactoryOptions options, JwtSecurityTokenHandler jwtSecurityTokenHandler)
    {
        _options = options;
        _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
    }

    public TokenResult Create(UserTokenClaims user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var jwt = CreateToken(
            signingCredentials,
            [
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(ClaimTypes.Email, user.Email)
            ],
            DateTime.UtcNow.AddDays(_options.ExpiresDays)
        );

        var refreshJwt = CreateToken(
            signingCredentials,
            [
                new(ClaimTypes.Name, user.Login)
            ],
            DateTime.UtcNow.AddDays(_options.RefreshExpiresDays)
        );

        return new()
        {
            RefreshToken = refreshJwt,
            Token = jwt
        };
    }

    public TokenResult Create()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var jwt = CreateToken(
            signingCredentials,
            [
                new(ClaimTypes.Role, nameof(Role.Service))
            ],
            DateTime.UtcNow.AddDays(_options.ExpiresDays)
        );

        var refreshJwt = CreateToken(
            signingCredentials,
            new()
            {
                new(ClaimTypes.Role, nameof(Role.Service)),
            },
            DateTime.UtcNow.AddDays(_options.RefreshExpiresDays)
        );

        return new()
        {
            RefreshToken = refreshJwt,
            Token = jwt
        };
    }

    private string CreateToken(SigningCredentials signingCredentials, List<Claim> claims, DateTime expires)
    {
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        var jwt = _jwtSecurityTokenHandler.WriteToken(token);

        return jwt;
    }
}