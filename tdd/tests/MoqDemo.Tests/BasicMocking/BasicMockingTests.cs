using FluentAssertions;
using Moq;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.BasicMocking;

/// <summary>
/// Demonstrates basic mocking strategies with Moq
/// </summary>
public class BasicMockingTests
{
    [Fact]
    public async Task Mock_SimpleMethod_ReturnsConfiguredValue()
    {
        // Arrange - Create a mock of the repository
        var mockRepository = new Mock<IUserRepository>();

        var expectedUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Setup - Configure the mock to return a specific value
        mockRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await mockRepository.Object.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedUser);
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Mock_WithDifferentParameters_ReturnsDifferentValues()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        var user1 = new User { Id = 1, Email = "user1@example.com" };
        var user2 = new User { Id = 2, Email = "user2@example.com" };

        // Setup multiple method calls with different parameters
        mockRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user1);
        mockRepository.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(user2);
        mockRepository.Setup(repo => repo.GetByIdAsync(3)).ReturnsAsync((User?)null);

        // Act & Assert
        var result1 = await mockRepository.Object.GetByIdAsync(1);
        result1.Should().Be(user1);

        var result2 = await mockRepository.Object.GetByIdAsync(2);
        result2.Should().Be(user2);

        var result3 = await mockRepository.Object.GetByIdAsync(3);
        result3.Should().BeNull();
    }

    [Fact]
    public async Task Mock_WithItIsAny_MatchesAnyParameter()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        var defaultUser = new User { Id = 999, Email = "default@example.com" };

        // Setup using It.IsAny<T>() to match any parameter
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(defaultUser);

        // Act
        var result1 = await mockRepository.Object.GetByIdAsync(1);
        var result2 = await mockRepository.Object.GetByIdAsync(100);
        var result3 = await mockRepository.Object.GetByIdAsync(-1);

        // Assert - All calls return the same configured value
        result1.Should().Be(defaultUser);
        result2.Should().Be(defaultUser);
        result3.Should().Be(defaultUser);
    }

    [Fact]
    public async Task Mock_WithItIs_MatchesConditionally()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();

        var expensiveProduct = new Product { Id = 1, Price = 1000, Name = "Expensive" };
        var cheapProduct = new Product { Id = 2, Price = 10, Name = "Cheap" };

        // Setup with conditional matching
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id > 100)))
            .ReturnsAsync(expensiveProduct);

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id <= 100)))
            .ReturnsAsync(cheapProduct);

        // Act
        var result1 = await mockRepository.Object.GetByIdAsync(1);
        var result2 = await mockRepository.Object.GetByIdAsync(200);

        // Assert
        result1.Should().Be(cheapProduct);
        result2.Should().Be(expensiveProduct);
    }

    [Fact]
    public async Task Mock_ReturnsTask_ForAsyncMethods()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var users = new List<User>
        {
            new User { Id = 1, Email = "user1@example.com" },
            new User { Id = 2, Email = "user2@example.com" }
        };

        // Setup async method to return Task<IEnumerable<T>>
        mockRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await mockRepository.Object.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task Mock_ThrowsException_WhenConfigured()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Setup to throw an exception
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var act = async () => await mockRepository.Object.GetByIdAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Mock_ReturnsValue_BasedOnInputParameter()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Setup using Returns with a function that uses the input parameter
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new User
            {
                Id = id,
                Email = $"user{id}@example.com"
            });

        // Act
        var user1 = await mockRepository.Object.GetByIdAsync(5);
        var user2 = await mockRepository.Object.GetByIdAsync(10);

        // Assert
        user1!.Id.Should().Be(5);
        user1.Email.Should().Be("user5@example.com");

        user2!.Id.Should().Be(10);
        user2.Email.Should().Be("user10@example.com");
    }

    [Fact]
    public async Task Mock_MethodReturningBool_ConfiguredForDifferentScenarios()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Setup for true case
        mockRepository
            .Setup(repo => repo.IsEmailUniqueAsync("unique@example.com"))
            .ReturnsAsync(true);

        // Setup for false case
        mockRepository
            .Setup(repo => repo.IsEmailUniqueAsync("duplicate@example.com"))
            .ReturnsAsync(false);

        // Act
        var isUnique = await mockRepository.Object.IsEmailUniqueAsync("unique@example.com");
        var isDuplicate = await mockRepository.Object.IsEmailUniqueAsync("duplicate@example.com");

        // Assert
        isUnique.Should().BeTrue();
        isDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task Mock_VoidMethod_CanBeSetupAndVerified()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();

        // Setup void/Task method
        mockEmailService
            .Setup(service => service.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await mockEmailService.Object.SendEmailAsync("test@example.com", "Subject", "Body");

        // Assert - Verify the method was called
        mockEmailService.Verify(
            service => service.SendEmailAsync("test@example.com", "Subject", "Body"),
            Times.Once);
    }
}
