using FluentAssertions;
using Moq;
using MoqDemo.Core.Interfaces;
using Xunit;

namespace MoqDemo.Tests.AdvancedMocking;

/// <summary>
/// Demonstrates property and event mocking with Moq
/// </summary>
public class PropertyAndEventTests
{
    [Fact]
    public void Mock_Properties_UsingSetupProperty()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();

        // This would work if IUserRepository had properties
        // For demonstration, we'll use a simpler example
        var mock = new Mock<ITestInterface>();
        mock.SetupProperty(x => x.Name, "Initial Value");

        // Act
        var initialValue = mock.Object.Name;
        mock.Object.Name = "Updated Value";
        var updatedValue = mock.Object.Name;

        // Assert
        initialValue.Should().Be("Initial Value");
        updatedValue.Should().Be("Updated Value");
    }

    [Fact]
    public void Mock_AllProperties_UsingSetupAllProperties()
    {
        // Arrange
        var mock = new Mock<ITestInterface>();
        mock.SetupAllProperties();

        // Act
        mock.Object.Name = "Test Name";
        mock.Object.Count = 42;

        // Assert
        mock.Object.Name.Should().Be("Test Name");
        mock.Object.Count.Should().Be(42);
    }

    [Fact]
    public async Task Mock_Events_RaisingEvents()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationService>();
        string? capturedMessage = null;

        // Subscribe to the event
        mockNotificationService.Object.NotificationSent += (sender, message) =>
        {
            capturedMessage = message;
        };

        // Act - Raise the event
        mockNotificationService.Raise(x => x.NotificationSent += null, "Test notification");

        // Assert
        capturedMessage.Should().Be("Test notification");
    }

    [Fact]
    public async Task Mock_Events_VerifyEventSubscription()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationService>();
        var eventHandlerCalled = false;

        EventHandler<string> handler = (sender, message) =>
        {
            eventHandlerCalled = true;
        };

        // Act
        mockNotificationService.Object.NotificationSent += handler;
        mockNotificationService.Raise(x => x.NotificationSent += null, "Test");

        // Assert
        eventHandlerCalled.Should().BeTrue();
    }

    [Fact]
    public void Mock_ReadOnlyProperty_ReturnsConfiguredValue()
    {
        // Arrange - Using test interface with properties
        var mock = new Mock<ITestInterface>();
        mock.Setup(x => x.ReadOnlyValue).Returns(100);

        // Act
        var value = mock.Object.ReadOnlyValue;

        // Assert
        value.Should().Be(100);
    }

    [Fact]
    public void Mock_Property_GetAndSet_Tracking()
    {
        // Arrange
        var mock = new Mock<ITestInterface>();
        mock.SetupProperty(x => x.Name);

        // Act
        mock.Object.Name = "Test";
        var value = mock.Object.Name;

        // Assert - Verify getter was called
        mock.VerifyGet(x => x.Name, Times.Once);

        // Verify setter was called
        mock.VerifySet(x => x.Name = "Test", Times.Once);
    }

    [Fact]
    public void Mock_PropertySetter_WithConditional()
    {
        // Arrange
        var mock = new Mock<ITestInterface>();
        mock.SetupSet(x => x.Count = It.IsInRange(1, 100, Moq.Range.Inclusive))
            .Verifiable();

        // Act
        mock.Object.Count = 50;

        // Assert
        mock.Verify();
    }

    [Fact]
    public void Mock_Property_ThrowsOnInvalidValue()
    {
        // Arrange
        var mock = new Mock<ITestInterface>();
        mock.SetupSet(x => x.Count = It.Is<int>(v => v < 0))
            .Throws<ArgumentException>();

        // Act & Assert
        var act = () => mock.Object.Count = -1;
        act.Should().Throw<ArgumentException>();
    }

    // Helper interface for property testing
    public interface ITestInterface
    {
        string Name { get; set; }
        int Count { get; set; }
        int ReadOnlyValue { get; }
    }
}
