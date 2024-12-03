using System.IdentityModel.Tokens.Jwt;
using System.Text;
using L_Bank_W_Backend;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace L_Bank.Test;

public class LoginServiceTests
{
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly JwtSettings _jwtSettings;

    public LoginServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            PrivateKey = "ThisIsASecretKeyForJwtTesting1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _jwtSettingsMock.Setup(x => x.Value).Returns(_jwtSettings);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenPrivateKeyIsNull()
    {
        // Arrange
        _jwtSettings.PrivateKey = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LoginService(_jwtSettingsMock.Object));
    }

    [Fact]
    public void CreateJwt_ShouldThrowArgumentNullException_WhenUserIsNull()
    {
        // Arrange
        var loginService = new LoginService(_jwtSettingsMock.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loginService.CreateJwt(null));
    }

    [Fact]
    public void CreateJwt_ShouldReturnValidJwt_WhenUserIsValid()
    {
        // Arrange
        var loginService = new LoginService(_jwtSettingsMock.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser"
        };

        // Act
        var token = loginService.CreateJwt(user);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.PrivateKey!);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        handler.ValidateToken(token, validationParameters, out var validatedToken);
        Assert.NotNull(validatedToken);
        Assert.IsType<JwtSecurityToken>(validatedToken);
    }
}