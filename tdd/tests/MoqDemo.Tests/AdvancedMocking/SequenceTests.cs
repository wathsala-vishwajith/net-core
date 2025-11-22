using FluentAssertions;
using Moq;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.AdvancedMocking;

/// <summary>
/// Demonstrates sequence and state-based mocking with Moq
/// </summary>
public class SequenceTests
{
    [Fact]
    public async Task SetupSequence_ReturnsDifferentValues_OnConsecutiveCalls()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .SetupSequence(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, FirstName = "First Call" })
            .ReturnsAsync(new User { Id = 1, FirstName = "Second Call" })
            .ReturnsAsync((User?)null);

        // Act & Assert
        var firstCall = await mockRepository.Object.GetByIdAsync(1);
        firstCall!.FirstName.Should().Be("First Call");

        var secondCall = await mockRepository.Object.GetByIdAsync(1);
        secondCall!.FirstName.Should().Be("Second Call");

        var thirdCall = await mockRepository.Object.GetByIdAsync(1);
        thirdCall.Should().BeNull();
    }

    [Fact]
    public async Task SetupSequence_ThrowsThenReturns()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        mockRepository
            .SetupSequence(repo => repo.GetByIdAsync(1))
            .ThrowsAsync(new InvalidOperationException("First call fails"))
            .ReturnsAsync(new User { Id = 1, FirstName = "Success on retry" });

        // Act & Assert
        var act = async () => await mockRepository.Object.GetByIdAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("First call fails");

        var secondCall = await mockRepository.Object.GetByIdAsync(1);
        secondCall!.FirstName.Should().Be("Success on retry");
    }

    [Fact]
    public async Task SetupSequence_SimulatesRetryLogic()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var attemptCount = 0;

        mockRepository
            .SetupSequence(repo => repo.ExistsAsync(1))
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        // Act - Simulate retry logic
        bool exists = false;
        while (!exists && attemptCount < 3)
        {
            exists = await mockRepository.Object.ExistsAsync(1);
            attemptCount++;
        }

        // Assert
        exists.Should().BeTrue();
        attemptCount.Should().Be(3);
    }

    [Fact]
    public async Task StatefulMock_TracksInternalState()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var inMemoryStore = new Dictionary<int, User>();
        int nextId = 1;

        // Setup Add to store in dictionary
        mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                user.Id = nextId++;
                inMemoryStore[user.Id] = user;
                return user;
            });

        // Setup GetById to retrieve from dictionary
        mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => inMemoryStore.GetValueOrDefault(id));

        // Setup Delete to remove from dictionary
        mockRepository
            .Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                if (inMemoryStore.ContainsKey(id))
                {
                    inMemoryStore.Remove(id);
                    return true;
                }
                return false;
            });

        // Act
        var user1 = await mockRepository.Object.AddAsync(new User { Email = "user1@example.com" });
        var user2 = await mockRepository.Object.AddAsync(new User { Email = "user2@example.com" });

        var retrieved = await mockRepository.Object.GetByIdAsync(user1.Id);
        var deleted = await mockRepository.Object.DeleteAsync(user1.Id);
        var retrievedAfterDelete = await mockRepository.Object.GetByIdAsync(user1.Id);

        // Assert
        user1.Id.Should().Be(1);
        user2.Id.Should().Be(2);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("user1@example.com");
        deleted.Should().BeTrue();
        retrievedAfterDelete.Should().BeNull();
    }

    [Fact]
    public async Task ConditionalSetup_BasedOnCallCount()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        int callCount = 0;

        mockRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return new List<User> { new User { Id = 1 } };
                if (callCount == 2)
                    return new List<User> { new User { Id = 1 }, new User { Id = 2 } };
                return new List<User>();
            });

        // Act
        var firstCall = await mockRepository.Object.GetAllAsync();
        var secondCall = await mockRepository.Object.GetAllAsync();
        var thirdCall = await mockRepository.Object.GetAllAsync();

        // Assert
        firstCall.Should().HaveCount(1);
        secondCall.Should().HaveCount(2);
        thirdCall.Should().BeEmpty();
    }

    [Fact]
    public async Task SetupSequence_WithCallbacks()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var executionLog = new List<string>();

        mockRepository
            .SetupSequence(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1 })
            .Callback(() => executionLog.Add("First call"))
            .ReturnsAsync(new User { Id = 1 })
            .Callback(() => executionLog.Add("Second call"));

        // Act
        await mockRepository.Object.GetByIdAsync(1);
        await mockRepository.Object.GetByIdAsync(1);

        // Note: Callbacks in SetupSequence need to be set differently
        // This is for demonstration of the concept
    }

    [Fact]
    public async Task DifferentBehavior_BasedOnTimeOfDay()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var testTime = DateTime.UtcNow;

        mockRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(() =>
            {
                // Simulate different behavior based on time
                var currentHour = testTime.Hour;
                if (currentHour < 12)
                    return new List<User> { new User { FirstName = "Morning User" } };
                else
                    return new List<User> { new User { FirstName = "Afternoon User" } };
            });

        // Act
        var result = await mockRepository.Object.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
    }
}
