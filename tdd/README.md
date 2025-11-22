# Moq Demonstration Project

A comprehensive .NET Core project demonstrating different mocking strategies with Moq, including integration with dependency injection and best practices for testable code.

## 📋 Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Mocking Strategies](#mocking-strategies)
- [Best Practices](#best-practices)
- [Running Tests](#running-tests)
- [Key Concepts](#key-concepts)

## 🎯 Overview

This project demonstrates:

- ✅ Basic mocking with Moq
- ✅ Verification strategies
- ✅ Advanced mocking techniques (callbacks, sequences, properties, events)
- ✅ Exception and async patterns
- ✅ Integration with Dependency Injection
- ✅ Best practices for writing testable code
- ✅ Real-world service layer examples

## 📁 Project Structure

```
tdd/
├── src/
│   ├── MoqDemo.Core/              # Domain entities and interfaces
│   │   ├── Entities/              # User, Product, Order entities
│   │   └── Interfaces/            # Repository and service interfaces
│   └── MoqDemo.Application/       # Service implementations
│       └── Services/              # UserService, OrderService, ProductService
└── tests/
    └── MoqDemo.Tests/
        ├── BasicMocking/          # Basic mocking examples
        ├── AdvancedMocking/       # Callbacks, sequences, events
        ├── IntegrationTests/      # DI integration examples
        └── BestPractices/         # Best practices demonstrations
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Installation

```bash
# Clone the repository
cd tdd

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## 🎨 Mocking Strategies

### 1. Basic Mocking

**File:** `tests/MoqDemo.Tests/BasicMocking/BasicMockingTests.cs`

#### Simple Method Setup

```csharp
var mockRepository = new Mock<IUserRepository>();

mockRepository
    .Setup(repo => repo.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Email = "test@example.com" });

var result = await mockRepository.Object.GetByIdAsync(1);
```

#### Using Parameter Matchers

```csharp
// Match any parameter
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(defaultUser);

// Conditional matching
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id > 100)))
    .ReturnsAsync(expensiveProduct);
```

#### Dynamic Return Values

```csharp
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => new User { Id = id, Email = $"user{id}@example.com" });
```

### 2. Verification Strategies

**File:** `tests/MoqDemo.Tests/BasicMocking/VerificationTests.cs`

#### Verify Method Calls

```csharp
// Verify called exactly once
mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);

// Verify called multiple times
mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.Exactly(3));

// Verify never called
mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);

// Verify at least/at most
mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.AtLeastOnce());
mockRepository.Verify(repo => repo.GetByIdAsync(1), Times.AtMostOnce());
```

#### Verify With Specific Parameters

```csharp
mockEmailService.Verify(
    s => s.SendEmailAsync(
        It.Is<string>(email => email.Contains("@")),
        It.IsAny<string>(),
        It.IsAny<string>()),
    Times.Once);
```

### 3. Callbacks

**File:** `tests/MoqDemo.Tests/AdvancedMocking/CallbackTests.cs`

#### Execute Custom Logic

```csharp
var callbackExecuted = false;

mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .Callback(() => callbackExecuted = true)
    .ReturnsAsync(new User { Id = 1 });
```

#### Capture Parameters

```csharp
int capturedId = 0;

mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .Callback<int>(id => capturedId = id)
    .ReturnsAsync(new User());
```

#### Modify State

```csharp
mockRepository
    .Setup(repo => repo.AddAsync(It.IsAny<User>()))
    .Callback<User>(user => {
        user.Id = 100;
        user.CreatedAt = DateTime.UtcNow;
    })
    .ReturnsAsync((User user) => user);
```

### 4. Sequences

**File:** `tests/MoqDemo.Tests/AdvancedMocking/SequenceTests.cs`

#### Return Different Values on Consecutive Calls

```csharp
mockRepository
    .SetupSequence(repo => repo.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, FirstName = "First Call" })
    .ReturnsAsync(new User { Id = 1, FirstName = "Second Call" })
    .ReturnsAsync((User?)null);
```

#### Simulate Retry Logic

```csharp
mockRepository
    .SetupSequence(repo => repo.ExistsAsync(1))
    .ThrowsAsync(new TimeoutException("Connection timeout"))
    .ReturnsAsync(true);
```

#### Stateful Mocks

```csharp
var inMemoryStore = new Dictionary<int, User>();

mockRepository
    .Setup(repo => repo.AddAsync(It.IsAny<User>()))
    .ReturnsAsync((User user) => {
        user.Id = nextId++;
        inMemoryStore[user.Id] = user;
        return user;
    });

mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => inMemoryStore.GetValueOrDefault(id));
```

### 5. Properties and Events

**File:** `tests/MoqDemo.Tests/AdvancedMocking/PropertyAndEventTests.cs`

#### Mock Properties

```csharp
var mock = new Mock<ITestInterface>();
mock.SetupProperty(x => x.Name, "Initial Value");

mock.Object.Name = "Updated Value";

// Verify property access
mock.VerifyGet(x => x.Name, Times.Once);
mock.VerifySet(x => x.Name = "Updated Value", Times.Once);
```

#### Raise Events

```csharp
mockNotificationService.Object.NotificationSent += (sender, message) => {
    capturedMessage = message;
};

mockNotificationService.Raise(x => x.NotificationSent += null, "Test notification");
```

### 6. Exception Handling

**File:** `tests/MoqDemo.Tests/AdvancedMocking/ExceptionAndAsyncTests.cs`

#### Throw Exceptions

```csharp
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .ThrowsAsync(new KeyNotFoundException("User not found"));
```

#### Conditional Exceptions

```csharp
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id < 0)))
    .ThrowsAsync(new ArgumentException("ID must be positive"));
```

### 7. Async Patterns

**File:** `tests/MoqDemo.Tests/AdvancedMocking/ExceptionAndAsyncTests.cs`

#### Mock Async Methods

```csharp
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new User { Id = 1 });
```

#### Simulate Delays

```csharp
mockRepository
    .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
    .Returns(async () => {
        await Task.Delay(100);
        return new User { Id = 1 };
    });
```

### 8. Integration with Dependency Injection

**File:** `tests/MoqDemo.Tests/IntegrationTests/DependencyInjectionTests.cs`

#### Register Mocks in Service Collection

```csharp
var serviceProvider = new ServiceCollection()
    .AddSingleton(mockUserRepository.Object)
    .AddSingleton(mockEmailService.Object)
    .AddLogging()
    .AddTransient<UserService>()
    .BuildServiceProvider();

var userService = serviceProvider.GetRequiredService<UserService>();
```

#### Test Service Factory Pattern

```csharp
public class TestServiceFactory
{
    public UserService CreateUserService(
        Func<int, Task<User?>>? getUserById = null)
    {
        var mockUserRepository = new Mock<IUserRepository>();

        if (getUserById != null)
        {
            mockUserRepository
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .Returns(getUserById);
        }

        return new UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object);
    }
}
```

## 🏆 Best Practices

**File:** `tests/MoqDemo.Tests/BestPractices/BestPracticesTests.cs`

### 1. Follow Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task UserService_CreateUser_Success()
{
    // Arrange - Set up all preconditions
    var mockRepository = new Mock<IUserRepository>();
    var service = new UserService(mockRepository.Object);

    // Act - Execute the method under test
    var result = await service.CreateUserAsync(newUser);

    // Assert - Verify the expected outcome
    result.Should().NotBeNull();
}
```

### 2. Use Meaningful Test Names

```csharp
// ✅ Good: ComponentName_MethodName_Scenario_ExpectedResult
[Fact]
public async Task UserService_CreateUser_WithDuplicateEmail_ThrowsException()

// ❌ Bad: Unclear what is being tested
[Fact]
public async Task Test1()
```

### 3. Mock Only What You Need

```csharp
// ✅ Good: Mock external dependencies
var mockRepository = new Mock<IUserRepository>();

// ✅ Good: Use real value objects
var user = new User { Id = 1, Email = "test@example.com" };

// ❌ Bad: Don't mock value objects or DTOs
var mockUser = new Mock<User>();
```

### 4. Don't Mock What You Don't Own

```csharp
// ✅ Good: Mock your own interfaces
var mockUserRepository = new Mock<IUserRepository>();

// ❌ Bad: Don't mock framework types directly
var mockDbContext = new Mock<DbContext>();
```

### 5. Verify Important Interactions

```csharp
// Verify critical operations
mockRepository.Verify(
    repo => repo.AddAsync(It.IsAny<User>()),
    Times.Once,
    "User should be saved to repository");

mockEmailService.Verify(
    s => s.SendWelcomeEmailAsync(user.Email, user.FullName),
    Times.Once,
    "Welcome email should be sent");
```

### 6. Use Specific Matchers

```csharp
// ✅ Good: Specific matchers
mockRepository.Setup(repo =>
    repo.GetByEmailAsync(It.Is<string>(email => email.Contains("@"))))
    .ReturnsAsync(user);

// ❌ Less ideal: Too generic
mockRepository.Setup(repo =>
    repo.GetByEmailAsync(It.IsAny<string>()))
    .ReturnsAsync(user);
```

### 7. One Behavior Per Test

```csharp
// ✅ Good: Test one specific behavior
[Fact]
public async Task UserService_CreateUser_SetsIsActiveToTrue()
{
    var result = await service.CreateUserAsync(user);
    result.IsActive.Should().BeTrue();
}

// ✅ Good: Separate test for different behavior
[Fact]
public async Task UserService_CreateUser_SetsCreatedAtToCurrentTime()
{
    var result = await service.CreateUserAsync(user);
    result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}
```

### 8. Use Helper Methods for Complex Setup

```csharp
private (UserService service, Mock<IUserRepository> repo) CreateUserServiceWithMocks()
{
    var mockRepository = new Mock<IUserRepository>();
    var mockEmailService = new Mock<IEmailService>();
    var mockLogger = new Mock<ILogger<UserService>>();

    var service = new UserService(
        mockRepository.Object,
        mockEmailService.Object,
        mockLogger.Object);

    return (service, mockRepository);
}
```

### 9. Test Edge Cases

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(int.MinValue)]
public async Task UserService_GetById_WithInvalidId_ReturnsNull(int invalidId)
{
    var result = await service.GetUserByIdAsync(invalidId);
    result.Should().BeNull();
}
```

### 10. Keep Tests Maintainable

- Write self-documenting tests
- Avoid complex logic in tests
- Keep tests independent
- Use constants for magic values
- Don't repeat yourself (DRY) - but prefer clarity over DRY

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~BasicMockingTests"

# Run tests in a specific namespace
dotnet test --filter "FullyQualifiedName~MoqDemo.Tests.BasicMocking"

# Generate code coverage
dotnet test /p:CollectCoverage=true
```

## 📚 Key Concepts

### Mock Behaviors

```csharp
// Loose (default): Returns default values for unexpected calls
var looseMock = new Mock<IUserRepository>();

// Strict: Throws exception for unexpected calls
var strictMock = new Mock<IUserRepository>(MockBehavior.Strict);
```

### Argument Matching

| Matcher | Description | Example |
|---------|-------------|---------|
| `It.IsAny<T>()` | Matches any value of type T | `It.IsAny<int>()` |
| `It.Is<T>(predicate)` | Matches values satisfying predicate | `It.Is<int>(x => x > 0)` |
| `It.IsInRange<T>(from, to, range)` | Matches values in range | `It.IsInRange(1, 100, Range.Inclusive)` |
| `It.IsRegex(pattern)` | Matches strings matching regex | `It.IsRegex(@"^\d{3}$")` |

### Verification Times

| Times | Description |
|-------|-------------|
| `Times.Once()` | Exactly one call |
| `Times.Never()` | Zero calls |
| `Times.Exactly(n)` | Exactly n calls |
| `Times.AtLeastOnce()` | One or more calls |
| `Times.AtMostOnce()` | Zero or one call |
| `Times.Between(n, m, Range)` | Between n and m calls |

## 🎓 Learning Path

1. **Start with Basics** → `BasicMocking/BasicMockingTests.cs`
2. **Learn Verification** → `BasicMocking/VerificationTests.cs`
3. **Master Callbacks** → `AdvancedMocking/CallbackTests.cs`
4. **Understand Sequences** → `AdvancedMocking/SequenceTests.cs`
5. **Handle Exceptions** → `AdvancedMocking/ExceptionAndAsyncTests.cs`
6. **Properties & Events** → `AdvancedMocking/PropertyAndEventTests.cs`
7. **Integration Testing** → `IntegrationTests/ServiceIntegrationTests.cs`
8. **DI Integration** → `IntegrationTests/DependencyInjectionTests.cs`
9. **Follow Best Practices** → `BestPractices/BestPracticesTests.cs`

## 📖 Additional Resources

- [Moq GitHub Repository](https://github.com/moq/moq4)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)

## 🤝 Contributing

This is a demonstration project. Feel free to explore, learn, and adapt the patterns to your own projects.

## 📝 License

This project is for educational purposes.

---

**Happy Testing! 🚀**
