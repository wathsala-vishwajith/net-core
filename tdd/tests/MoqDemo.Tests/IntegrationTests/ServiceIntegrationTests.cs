using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoqDemo.Application.Services;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.IntegrationTests;

/// <summary>
/// Demonstrates integration testing with services and multiple dependencies
/// </summary>
public class ServiceIntegrationTests
{
    [Fact]
    public async Task UserService_CreateUser_Success()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        var newUser = new User
        {
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        mockUserRepository
            .Setup(repo => repo.IsEmailUniqueAsync(newUser.Email))
            .ReturnsAsync(true);

        mockUserRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });

        mockEmailService
            .Setup(s => s.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        // Act
        var result = await service.CreateUserAsync(newUser);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify all interactions
        mockUserRepository.Verify(repo => repo.IsEmailUniqueAsync(newUser.Email), Times.Once);
        mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        mockEmailService.Verify(s => s.SendWelcomeEmailAsync(newUser.Email, newUser.FullName), Times.Once);
    }

    [Fact]
    public async Task UserService_CreateUser_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        mockUserRepository
            .Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var service = new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        var duplicateUser = new User { Email = "duplicate@example.com" };

        // Act
        var act = async () => await service.CreateUserAsync(duplicateUser);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");

        // Verify email was never sent
        mockEmailService.Verify(
            s => s.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OrderService_CreateOrder_Success()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderService>>();

        var user = new User { Id = 1, Email = "user@example.com", IsActive = true };
        var product = new Product { Id = 1, Price = 100, StockQuantity = 10 };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);
        mockProductRepository.Setup(repo => repo.UpdateStockAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        mockOrderRepository.Setup(repo => repo.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => { o.Id = 1; return o; });
        mockEmailService.Setup(s => s.SendOrderConfirmationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(true);
        mockNotificationService.Setup(s => s.NotifyAdminAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object,
            mockUserRepository.Object,
            mockEmailService.Object,
            mockNotificationService.Object,
            mockLogger.Object);

        var items = new List<(int ProductId, int Quantity)> { (1, 2) };

        // Act
        var result = await service.CreateOrderAsync(1, items);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.TotalAmount.Should().Be(200);
        result.Items.Should().HaveCount(1);

        // Verify all dependencies were called
        mockUserRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        mockProductRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        mockProductRepository.Verify(repo => repo.UpdateStockAsync(1, 8), Times.Once);
        mockEmailService.Verify(s => s.SendOrderConfirmationAsync(user.Email, 1, 200), Times.Once);
        mockNotificationService.Verify(s => s.NotifyAdminAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task OrderService_CreateOrder_InsufficientStock_ThrowsException()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockLogger = new Mock<ILogger<OrderService>>();

        var user = new User { Id = 1, IsActive = true };
        var product = new Product { Id = 1, Price = 100, StockQuantity = 1, Name = "Limited Product" };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object,
            mockUserRepository.Object,
            mockEmailService.Object,
            mockNotificationService.Object,
            mockLogger.Object);

        var items = new List<(int ProductId, int Quantity)> { (1, 5) };

        // Act
        var act = async () => await service.CreateOrderAsync(1, items);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");

        // Verify order was never created
        mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never);
        mockEmailService.Verify(
            s => s.SendOrderConfirmationAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
            Times.Never);
    }

    [Fact]
    public async Task ProductService_CreateProduct_ValidatesPrice()
    {
        // Arrange
        var mockProductRepository = new Mock<IProductRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        var service = new ProductService(mockProductRepository.Object, mockLogger.Object);

        var invalidProduct = new Product { Name = "Test", Price = -10 };

        // Act
        var act = async () => await service.CreateProductAsync(invalidProduct);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Price must be greater than zero*");

        // Verify repository was never called
        mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UserService_DeactivateUser_Success()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        var existingUser = new User { Id = 1, Email = "user@example.com", IsActive = true };

        mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(existingUser);
        mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);

        var service = new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        // Act
        var result = await service.DeactivateUserAsync(1);

        // Assert
        result.Should().BeTrue();
        existingUser.IsActive.Should().BeFalse();

        mockUserRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.IsActive == false)), Times.Once);
    }
}
