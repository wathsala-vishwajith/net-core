using FluentAssertions;
using Moq;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.AdvancedMocking;

/// <summary>
/// Demonstrates callback functionality in Moq
/// </summary>
public class CallbackTests
{
    [Fact]
    public async Task Callback_ExecutesCustomLogic_BeforeReturningValue()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var callbackExecuted = false;

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Callback(() => callbackExecuted = true)
            .ReturnsAsync(new User { Id = 1 });

        // Act
        await mockRepository.Object.GetByIdAsync(1);

        // Assert
        callbackExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Callback_CapturesMethodParameters()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        int capturedId = 0;

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Callback<int>(id => capturedId = id)
            .ReturnsAsync(new User());

        // Act
        await mockRepository.Object.GetByIdAsync(42);

        // Assert
        capturedId.Should().Be(42);
    }

    [Fact]
    public async Task Callback_ModifiesStateBeforeReturn()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var executionLog = new List<string>();

        mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                executionLog.Add($"Adding user: {user.Email}");
                user.Id = 100; // Simulate ID assignment
            })
            .ReturnsAsync((User user) => user);

        // Act
        var newUser = new User { Email = "test@example.com" };
        var result = await mockRepository.Object.AddAsync(newUser);

        // Assert
        executionLog.Should().Contain("Adding user: test@example.com");
        result.Id.Should().Be(100);
    }

    [Fact]
    public async Task Callback_WithMultipleParameters()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var capturedRecipient = string.Empty;
        var capturedSubject = string.Empty;
        var capturedBody = string.Empty;

        mockEmailService
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((to, subject, body) =>
            {
                capturedRecipient = to;
                capturedSubject = subject;
                capturedBody = body;
            })
            .ReturnsAsync(true);

        // Act
        await mockEmailService.Object.SendEmailAsync("user@example.com", "Welcome", "Hello World");

        // Assert
        capturedRecipient.Should().Be("user@example.com");
        capturedSubject.Should().Be("Welcome");
        capturedBody.Should().Be("Hello World");
    }

    [Fact]
    public async Task Callback_SimulatesAsyncDelay()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var startTime = DateTime.UtcNow;

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Callback(async () => await Task.Delay(100))
            .ReturnsAsync(new User());

        // Act
        await mockRepository.Object.GetByIdAsync(1);

        // Assert
        var elapsed = DateTime.UtcNow - startTime;
        elapsed.TotalMilliseconds.Should().BeGreaterOrEqualTo(100);
    }

    [Fact]
    public async Task Callback_TracksCallCount()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        int callCount = 0;

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Callback(() => callCount++)
            .ReturnsAsync(new User());

        // Act
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.GetByIdAsync(2);
        await mockRepository.Object.GetByIdAsync(3);

        // Assert
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task Callback_ValidatesInputParameters()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var validationErrors = new List<string>();

        mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                    validationErrors.Add("Email is required");
                if (string.IsNullOrWhiteSpace(user.FirstName))
                    validationErrors.Add("FirstName is required");
            })
            .ReturnsAsync((User user) => user);

        // Act
        await mockRepository.Object.AddAsync(new User { Email = "", FirstName = "" });

        // Assert
        validationErrors.Should().HaveCount(2);
        validationErrors.Should().Contain("Email is required");
        validationErrors.Should().Contain("FirstName is required");
    }

    [Fact]
    public async Task Callback_BuildsComplexReturnValue()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                // Simulate database behavior
                user.Id = new Random().Next(1, 1000);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
            })
            .ReturnsAsync((User user) => user);

        // Act
        var newUser = new User { Email = "test@example.com", FirstName = "John" };
        var result = await mockRepository.Object.AddAsync(newUser);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.IsActive.Should().BeTrue();
    }
}
