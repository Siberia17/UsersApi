using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;
using UsersApi.Models;

namespace UsersApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> LoginAsync(Login login)
        {
            return new User { email = login.email, passwordHash = login.password };
        }

        public async Task<User> RegisterAsync(User user)
        {
           //validar que no este registrado previamente
           var existingUser = await _userRepository.GetUserByEmailAsync(user.email);
            if (existingUser != null)
            {
                throw new Exception("El usuario ya esta registrado");
            }

            //Encriptar la contraseña del usuario
            var passwordHasher = new PasswordHasher<User>();
            user.passwordHash = passwordHasher.HashPassword(user, user.passwordHash);

            //Agregar el usuario a la base de datos
            await _userRepository.CreateUserAsync(user);
            return user;
        }
    }
}
