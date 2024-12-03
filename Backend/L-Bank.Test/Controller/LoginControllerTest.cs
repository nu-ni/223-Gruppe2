using L_Bank_W_Backend.Controllers;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Dto;
using L_Bank_W_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace L_Bank.Test;

public class LoginControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLoginService = new Mock<ILoginService>();
        _controller = new LoginController(_mockUserRepository.Object, _mockLoginService.Object);
    }

    [Fact]
    public async Task Post_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "invalidUser", Password = "wrongPassword" };
        _mockUserRepository.Setup(repo => repo.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((User?)null);

        // Act
        var result = await _controller.Post(loginDto);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Post_ShouldReturnToken_WhenUserIsAuthenticated()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "validUser", Password = "correctPassword" };
        var user = new User { Username = "validUser" };
        var token = "mockedToken";

        _mockUserRepository.Setup(repo => repo.Authenticate(It.Is<string>(u => u == "validUser"), It.Is<string>(p => p == "correctPassword")))
            .Returns(user);
        _mockLoginService.Setup(service => service.CreateJwt(It.IsAny<User>()))
            .Returns(token);

        // Act
        var result = await _controller.Post(loginDto) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(token, result.Value?.GetType().GetProperty("token")?.GetValue(result.Value));
    }
}