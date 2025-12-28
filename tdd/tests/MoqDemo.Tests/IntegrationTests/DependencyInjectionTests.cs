using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MoqDemo.Application.Services;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.IntegrationTests;

/// <summary>
/// Demonstrates integration of Moq with Microsoft.Extensions.DependencyInjection
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task ServiceProvider_WithMockedDependencies_WorksCorrectly()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();

        mockUserRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Email = "test@example.com" });

        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockUserRepository.Object)
            .AddSingleton(mockEmailService.Object)
            .AddLogging()
            .AddTransient<UserService>()
            .BuildServiceProvider();

        // Act
        var userService = serviceProvider.GetRequiredService<UserService>();
        var result = await userService.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void ServiceCollection_RegisterMocks_ForTesting()
    {
        // Arrange & Act
        var services = new ServiceCollection();

        // Register mocks as singletons
        services.AddSingleton(new Mock<IUserRepository>().Object);
        services.AddSingleton(new Mock<IProductRepository>().Object);
        services.AddSingleton(new Mock<IOrderRepository>().Object);
        services.AddSingleton(new Mock<IEmailService>().Object);
        services.AddSingleton(new Mock<INotificationService>().Object);

        // Register real services
        services.AddLogging();
        services.AddTransient<UserService>();
        services.AddTransient<ProductService>();
        services.AddTransient<OrderService>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert - All services can be resolved
        var userService = serviceProvider.GetService<UserService>();
        var productService = serviceProvider.GetService<ProductService>();
        var orderService = serviceProvider.GetService<OrderService>();

        userService.Should().NotBeNull();
        productService.Should().NotBeNull();
        orderService.Should().NotBeNull();
    }

    [Fact]
    public async Task TestServiceFactory_CreatesServiceWithMocks()
    {
        // Arrange
        var factory = new TestServiceFactory();
        var userService = factory.CreateUserService(
            getUserById: (id) => Task.FromResult<User?>(new User { Id = id }),
            isEmailUnique: (email) => Task.FromResult(true));

        // Act
        var result = await userService.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task ScopedServices_WithMocks_IsolatedPerScope()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        int callCount = 0;

        mockUserRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new User { Id = callCount };
            });

        var serviceProvider = new ServiceCollection()
            .AddSingleton(mockUserRepository.Object)
            .AddSingleton<Mock<IEmailService>>(new Mock<IEmailService>())
            .AddSingleton<IEmailService>(sp => sp.GetRequiredService<Mock<IEmailService>>().Object)
            .AddLogging()
            .AddScoped<UserService>()
            .BuildServiceProvider();

        // Act - Create two scopes
        User? user1, user2;
        using (var scope1 = serviceProvider.CreateScope())
        {
            var service1 = scope1.ServiceProvider.GetRequiredService<UserService>();
            user1 = await service1.GetUserByIdAsync(1);
        }

        using (var scope2 = serviceProvider.CreateScope())
        {
            var service2 = scope2.ServiceProvider.GetRequiredService<UserService>();
            user2 = await service2.GetUserByIdAsync(1);
        }

        // Assert - Both scopes used the same mock (singleton)
        user1!.Id.Should().Be(1);
        user2!.Id.Should().Be(2);
        callCount.Should().Be(2);
    }

    [Fact]
    public void MockFactory_CreatesConfiguredMocks()
    {
        // Arrange
        var factory = new MockFactory(MockBehavior.Strict);

        // Act
        var mockRepository = factory.Create<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 });

        // Assert - Strict mock will throw if unexpected method is called
        mockRepository.Object.GetByIdAsync(1).Wait();
    }

    [Fact]
    public async Task ServiceWithLogging_LoggerIsMocked()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();

        mockUserRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 });

        var service = new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        // Act
        await service.GetUserByIdAsync(1);

        // Assert - Verify logging occurred
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fetching user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

/// <summary>
/// Helper class for creating services with mock dependencies
/// </summary>
public class TestServiceFactory
{
    public UserService CreateUserService(
        Func<int, Task<User?>>? getUserById = null,
        Func<string, Task<bool>>? isEmailUnique = null)
    {
        var mockUserRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        if (getUserById != null)
        {
            mockUserRepository
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .Returns(getUserById);
        }

        if (isEmailUnique != null)
        {
            mockUserRepository
                .Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>()))
                .Returns(isEmailUnique);
        }

        return new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);
    }

    public ProductService CreateProductService(
        Func<int, Task<Product?>>? getProductById = null)
    {
        var mockProductRepository = new Mock<IProductRepository>();
        var mockLogger = new Mock<ILogger<ProductService>>();

        if (getProductById != null)
        {
            mockProductRepository
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .Returns(getProductById);
        }

        return new ProductService(
            mockProductRepository.Object,
            mockLogger.Object);
    }
}
