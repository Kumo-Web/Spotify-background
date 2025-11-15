using Domain.Entities;

namespace Application.Interfaces.repositories;

public interface IUserRepository
{
    Task<User> GetUserIdAsync(Guid userId);
    Task SaveAsync(User user);
    Task UpdateAsync(User user);
}
