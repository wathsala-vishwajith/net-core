using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MoqDemo.Application.Services;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.BestPractices;

/// <summary>
/// Demonstrates best practices for using Moq in testable code
/// </summary>
public class BestPracticesTests
{
    #region Arrange-Act-Assert Pattern

    [Fact]
    public async Task BestPractice_UseArrangeActAssertPattern()
    {
        // Arrange - Set up all preconditions and inputs
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Email = "test@example.com" });

        var service = new UserService(
            mockRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        // Act - Execute the method under test
        var result = await service.GetUserByIdAsync(1);

        // Assert - Verify the expected outcome
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    #endregion

    #region Mock Only What You Need

    [Fact]
    public async Task BestPractice_MockOnlyWhatYouNeed()
    {
        // Good: Only mock the repository, which is the external dependency
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 });

        // Don't mock value objects or DTOs - use real instances
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John"
        };

        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        var result = await mockRepository.Object.AddAsync(user);
        result.Should().Be(user);
    }

    #endregion

    #region Use Meaningful Test Names

    [Fact]
    public async Task UserService_CreateUser_WithValidData_ShouldSucceed()
    {
        // Test name clearly describes: Component_Method_Scenario_ExpectedResult
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        mockRepository.Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });
        mockEmailService.Setup(s => s.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new UserService(mockRepository.Object, mockEmailService.Object, mockLogger.Object);
        var user = new User { Email = "new@example.com", FirstName = "John", LastName = "Doe" };

        var result = await service.CreateUserAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Verify Important Interactions

    [Fact]
    public async Task BestPractice_VerifyImportantInteractions()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        mockRepository.Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });
        mockEmailService.Setup(s => s.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new UserService(mockRepository.Object, mockEmailService.Object, mockLogger.Object);
        var user = new User { Email = "test@example.com", FirstName = "John", LastName = "Doe" };

        // Act
        await service.CreateUserAsync(user);

        // Assert - Verify critical interactions
        mockRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once,
            "User should be saved to repository");

        mockEmailService.Verify(s => s.SendWelcomeEmailAsync(user.Email, user.FullName), Times.Once,
            "Welcome email should be sent to new user");
    }

    #endregion

    #region Use Specific Matchers

    [Fact]
    public async Task BestPractice_UseSpecificMatchers_InsteadOfItIsAny()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Good: Use specific matchers when you know the expected values
        mockRepository.Setup(repo => repo.GetByEmailAsync(It.Is<string>(email => email.Contains("@"))))
            .ReturnsAsync(new User { Email = "valid@example.com" });

        // Bad: Using It.IsAny when you could be more specific
        // mockRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))

        // Act
        var result = await mockRepository.Object.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Don't Mock What You Don't Own

    [Fact]
    public void BestPractice_DontMockWhatYouDontOwn()
    {
        // Good: Mock your own interfaces
        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User());

        // Bad: Don't mock framework types like DbContext directly
        // Instead, create your own repository interface
        // var mockDbContext = new Mock<DbContext>(); // Avoid this
    }

    #endregion

    #region One Assert Per Test (When Possible)

    [Fact]
    public async Task BestPractice_FocusOnSingleBehavior()
    {
        // Each test should verify one specific behavior
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, IsActive = true });

        var result = await mockRepository.Object.GetByIdAsync(1);

        // Focus on the specific behavior being tested
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task BestPractice_TestNotFoundScenarioSeparately()
    {
        // Separate test for different scenario
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        var result = await mockRepository.Object.GetByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region Use Helper Methods for Setup

    [Fact]
    public async Task BestPractice_UseHelperMethods_ForComplexSetup()
    {
        // Arrange
        var (service, mockRepository, mockEmailService) = CreateUserServiceWithMocks();

        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        var result = await service.GetUserByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    private (UserService service, Mock<IUserRepository> repo, Mock<IEmailService> email) CreateUserServiceWithMocks()
    {
        var mockRepository = new Mock<IUserRepository>();
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<UserService>>();

        var service = new UserService(
            mockRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);

        return (service, mockRepository, mockEmailService);
    }

    #endregion

    #region Test Edge Cases

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task BestPractice_TestEdgeCases_InvalidIds(int invalidId)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id <= 0)))
            .ReturnsAsync((User?)null);

        // Act
        var result = await mockRepository.Object.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Avoid Over-Mocking

    [Fact]
    public async Task BestPractice_AvoidOverMocking()
    {
        // Don't mock everything - use real objects when appropriate
        var mockRepository = new Mock<IUserRepository>();

        // Use real entity objects
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(user);

        var result = await mockRepository.Object.GetByIdAsync(1);

        // Real User object works fine in tests
        result!.FullName.Should().Be("John Doe");
    }

    #endregion

    #region Keep Tests Maintainable

    [Fact]
    public async Task BestPractice_KeepTestsSimpleAndReadable()
    {
        // Arrange - Clear setup
        var mockRepository = new Mock<IUserRepository>();
        var testUser = new User { Id = 1, Email = "test@example.com" };

        mockRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(testUser);

        // Act - Single, clear action
        var result = await mockRepository.Object.GetByIdAsync(1);

        // Assert - Clear verification
        result.Should().Be(testUser);

        // Tests should be easy to read and understand at a glance
    }

    #endregion
}
