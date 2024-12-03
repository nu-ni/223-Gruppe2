using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using L_Bank_W_Backend.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace L_Bank_W_Backend.Services;

public class LoginService : ILoginService
{
    private readonly JwtSettings jwtSettings;

    public LoginService(IOptions<JwtSettings> jwtSettings)
    {
        this.jwtSettings = jwtSettings.Value;
        
        if(string.IsNullOrWhiteSpace(this.jwtSettings.PrivateKey))
        {
            throw new ArgumentNullException(nameof(this.jwtSettings.PrivateKey));
        }
    }
    
    public string CreateJwt(User? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.PrivateKey!);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = credentials,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
    
    private static ClaimsIdentity GenerateClaims(User user)
    {
        if(user.Username == null)
        {
            throw new ArgumentNullException(nameof(user.Username));
        }
        
        var claims = new ClaimsIdentity();
        claims.AddClaim(new Claim(ClaimTypes.Name, user.Username));
        claims.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
        return claims;
    }

}