using UsersApi.Models;

namespace UsersApi.Services
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);   
        Task CreateUserAsync(User user);
    }
}