using L_Bank_W_Backend.Controllers;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class LedgersControllerTests
{
    private readonly Mock<ILedgerRepository> _mockRepo;
    private readonly LedgersController _controller;

    public LedgersControllerTests()
    {
        _mockRepo = new Mock<ILedgerRepository>();
        _controller = new LedgersController(_mockRepo.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        SetupMocks();
    }

    private void SetupMocks()
    {
        _mockRepo.Setup(repo => repo.CreateLedger(It.IsAny<string>()))
            .ReturnsAsync(3);

        _mockRepo.Setup(repo => repo.DeleteLedger(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private static List<Ledger> GetTestLedgers()
    {
        return
        [
            new Ledger { Id = 1, Name = "Ledger1" },
            new Ledger { Id = 2, Name = "Ledger2" }
        ];
    }

    [Fact]
    public async Task Post_ReturnsBadRequest_GivenInvalidModel()
    {
        // Arrange
        var createLedgerDto = new CreateLedgerDto { Name = null };

        // Act
        var result = await _controller.Post(createLedgerDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_ReturnsNewLedgerId()
    {
        // Arrange
        var createLedgerDto = new CreateLedgerDto { Name = "New Ledger" };

        // Act
        var result = await _controller.Post(createLedgerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(3, okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_GivenInvalidId()
    {
        // Act
        var result = await _controller.Delete(-1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_GivenExistingId()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.DeleteLedger(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(repo => repo.DeleteLedger(It.Is<int>(id => id != 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}