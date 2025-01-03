using Microsoft.AspNetCore.Identity.Data;
using UsersApi.Models;

namespace UsersApi.Services
{
    public interface IUserService
    {
        Task<User> LoginAsync(Login login);
        Task<User> RegisterAsync(User user);
    }
}
