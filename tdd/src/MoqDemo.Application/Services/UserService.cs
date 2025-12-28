using Microsoft.Extensions.Logging;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;

namespace MoqDemo.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Email is required", nameof(user));

        _logger.LogInformation("Creating user with email: {Email}", user.Email);

        var isUnique = await _userRepository.IsEmailUniqueAsync(user.Email);
        if (!isUnique)
        {
            _logger.LogWarning("Email already exists: {Email}", user.Email);
            throw new InvalidOperationException($"User with email {user.Email} already exists");
        }

        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        var createdUser = await _userRepository.AddAsync(user);

        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

        _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);

        return createdUser;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogInformation("Fetching all active users");
        return await _userRepository.GetActiveUsersAsync();
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        _logger.LogInformation("Deactivating user with ID: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User deactivated: {UserId}", userId);
        return true;
    }
}
