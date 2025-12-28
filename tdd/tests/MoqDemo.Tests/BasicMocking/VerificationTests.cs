using FluentAssertions;
using Moq;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.BasicMocking;

/// <summary>
/// Demonstrates verification strategies with Moq
/// </summary>
public class VerificationTests
{
    [Fact]
    public async Task Verify_MethodCalledOnce()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        await mockRepository.Object.GetByIdAsync(1);

        // Assert - Verify method was called exactly once
        mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task Verify_MethodCalledMultipleTimes()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.GetByIdAsync(1);

        // Assert
        mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Exactly(3));
    }

    [Fact]
    public async Task Verify_MethodNeverCalled()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Act - Intentionally not calling the method

        // Assert - Verify method was never called
        mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Verify_MethodCalledWithSpecificParameters()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await mockEmailService.Object.SendEmailAsync("test@example.com", "Hello", "Welcome!");

        // Assert - Verify with exact parameters
        mockEmailService.Verify(
            s => s.SendEmailAsync("test@example.com", "Hello", "Welcome!"),
            Times.Once);

        // Verify with matchers
        mockEmailService.Verify(
            s => s.SendEmailAsync(
                It.Is<string>(email => email.Contains("@")),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Verify_AtLeastOnce_And_AtMostOnce()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        await mockRepository.Object.GetByIdAsync(1);

        // Assert
        mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.AtLeastOnce());
        mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.AtMostOnce());
    }

    [Fact]
    public async Task Verify_Between_NumberOfCalls()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        await mockRepository.Object.ExistsAsync(1);
        await mockRepository.Object.ExistsAsync(1);

        // Assert - Between 1 and 3 calls
        mockRepository.Verify(
            repo => repo.ExistsAsync(1),
            Times.Between(1, 3, Moq.Range.Inclusive));
    }

    [Fact]
    public async Task VerifyAll_EnsuresAllSetupsWereCalled()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // Setup with Verifiable()
        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 })
            .Verifiable();

        mockRepository.Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.ExistsAsync(1);

        // Assert - Verify all setups marked as Verifiable were called
        mockRepository.Verify();
    }

    [Fact]
    public void VerifyNoOtherCalls_EnsuresOnlyExpectedCallsWereMade()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        mockRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 });

        // Act
        var user = mockRepository.Object.GetByIdAsync(1).Result;

        // Assert
        mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        mockRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Verify_OrderOfMethodCalls_UsingSequence()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var callSequence = new List<string>();

        mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User())
            .Callback(() => callSequence.Add("GetById"));

        mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(new User())
            .Callback(() => callSequence.Add("Update"));

        // Act
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.UpdateAsync(new User());

        // Assert
        callSequence.Should().Equal("GetById", "Update");
    }

    [Fact]
    public async Task Verify_PropertyAccess()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationService>();
        mockNotificationService.Setup(s => s.NotifyAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockNotificationService.Object.NotifyAsync("Test message");

        // Assert
        mockNotificationService.Verify(s => s.NotifyAsync("Test message"), Times.Once);
    }
}
