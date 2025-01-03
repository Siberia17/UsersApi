using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsersApi.Data;
using UsersApi.Models;

namespace UsersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration; 
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            try
            {
                using (SqlConnection connection = Conexion.GetConexion())
                {
                    connection.Open();
                    string query = "INSERT INTO Users (name, email, passwordHash) VALUES (@name, @email, @passwordHash)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", user.name);
                        command.Parameters.AddWithValue("@email", user.email);
                        command.Parameters.AddWithValue("@passwordHash", HashPassword(user.passwordHash));
                        command.ExecuteNonQuery();
                    }
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login(User user)
        {
            try
            {
                using (SqlConnection connection = Conexion.GetConexion())
                {
                    connection.Open();
                    string query = "SELECT * FROM Users WHERE email = @email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@email", user.email);
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string email = reader["email"].ToString();
                                string passwordHash = reader["PasswordHash"].ToString();

                                if(VerifyPassword(user.passwordHash, passwordHash))
                                {
                                    User user1 = new User()
                                    {
                                        idUser = (int)reader["idUser"],
                                        email = email
                                    };

                                    var token = GenerateToken(user);
                                    return Ok(new { token });
                                }
                                else
                                {
                                    return Unauthorized();
                                }
                            }
                            else
                            {
                                return Unauthorized();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message); 
            }
        }

        private string HashPassword(string passwordHash)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordHash));
                var builder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string passwordHash, string password)
        {
            return HashPassword(passwordHash) == password;
        } 

        private string GetJwtKey()
        {
            var key = _configuration["Jwt:Key"];
            if(string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("La configuracion de Jwt:Key no existe o esta vacia");
            }
            return key; 
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.Email, user.email)
            };

            var key = GetJwtKey();
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               issuer: _configuration["Jwt:Issuer"],
              audience:  _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
