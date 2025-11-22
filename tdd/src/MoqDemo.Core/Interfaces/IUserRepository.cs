using MoqDemo.Core.Entities;

namespace MoqDemo.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> IsEmailUniqueAsync(string email);
}
