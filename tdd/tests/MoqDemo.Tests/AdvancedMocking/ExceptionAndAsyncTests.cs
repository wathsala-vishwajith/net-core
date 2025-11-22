using FluentAssertions;
using Moq;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.AdvancedMocking;

/// <summary>
/// Demonstrates exception handling and async patterns with Moq
/// </summary>
public class ExceptionAndAsyncTests
{
    [Fact]
    public async Task Mock_ThrowsSpecificException()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var act = async () => await mockRepository.Object.GetByIdAsync(1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Mock_ThrowsException_BasedOnParameter()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id < 0)))
            .ThrowsAsync(new ArgumentException("ID must be positive"));

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id >= 0)))
            .ReturnsAsync(new User { Id = 1 });

        // Act & Assert
        var act = async () => await mockRepository.Object.GetByIdAsync(-1);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("ID must be positive");

        var validResult = await mockRepository.Object.GetByIdAsync(1);
        validResult.Should().NotBeNull();
    }

    [Fact]
    public async Task Mock_ThrowsException_OnlyOnFirstCall()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .SetupSequence(repo => repo.GetByIdAsync(1))
            .ThrowsAsync(new TimeoutException("Connection timeout"))
            .ReturnsAsync(new User { Id = 1 });

        // Act & Assert - First call throws
        var firstCall = async () => await mockRepository.Object.GetByIdAsync(1);
        await firstCall.Should().ThrowAsync<TimeoutException>();

        // Second call succeeds
        var secondCall = await mockRepository.Object.GetByIdAsync(1);
        secondCall.Should().NotBeNull();
    }

    [Fact]
    public async Task Mock_AsyncMethod_CompletesImmediately()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        var startTime = DateTime.UtcNow;
        var result = await mockRepository.Object.GetByIdAsync(1);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        result.Should().NotBeNull();
        elapsed.TotalMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task Mock_AsyncMethod_WithDelay()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                return new User { Id = 1 };
            });

        // Act
        var startTime = DateTime.UtcNow;
        var result = await mockRepository.Object.GetByIdAsync(1);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        result.Should().NotBeNull();
        elapsed.TotalMilliseconds.Should().BeGreaterOrEqualTo(100);
    }

    [Fact]
    public async Task Mock_CancellationToken_Honored()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var cts = new CancellationTokenSource();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .Returns(async () =>
            {
                await Task.Delay(1000, cts.Token);
                return new User();
            });

        // Act
        cts.CancelAfter(100);
        var act = async () => await mockRepository.Object.GetByIdAsync(1);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task Mock_MultipleAsyncCalls_InParallel()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new User { Id = id });

        // Act
        var tasks = new[]
        {
            mockRepository.Object.GetByIdAsync(1),
            mockRepository.Object.GetByIdAsync(2),
            mockRepository.Object.GetByIdAsync(3)
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(3);
        results[0]!.Id.Should().Be(1);
        results[1]!.Id.Should().Be(2);
        results[2]!.Id.Should().Be(3);
    }

    [Fact]
    public async Task Mock_ReturnsCompletedTask()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendEmailAsync("test@test.com", "Subject", "Body");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Mock_TaskFromResult()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var user = new User { Id = 1 };

        mockRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .Returns(Task.FromResult<User?>(user));

        // Act
        var result = await mockRepository.Object.GetByIdAsync(1);

        // Assert
        result.Should().Be(user);
    }

    [Fact]
    public async Task Mock_AggregateException_FromMultipleTasks()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Error"));

        // Act
        var tasks = new[]
        {
            mockRepository.Object.GetByIdAsync(1),
            mockRepository.Object.GetByIdAsync(2)
        };

        var act = async () => await Task.WhenAll(tasks);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
