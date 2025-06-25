using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UniversityBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public IActionResult Login([FromBody] Dictionary<string, object> loginData)
        {
            try
            {
                string email = loginData["email"].ToString();
                string password = loginData["password"].ToString();

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Users WHERE UserEmail = @Email AND UserPassword = @Password", connection);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // Kullanıcı bilgilerini oku
                        var userId = reader["UserId"].ToString();
                        var roleCode = reader["UserRoleCode"].ToString();
                        var userNo = reader["UserNo"].ToString();

                        // Token oluştur
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new[]
                            {
                                new Claim("UserId", userId),
                                new Claim("Email", email),
                                new Claim("Role", roleCode),
                                new Claim("UserNo", userNo),
                                //new Claim("Department", departmentCode)
                            }),
                            Expires = DateTime.UtcNow.AddHours(1),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                        };

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        string jwt = tokenHandler.WriteToken(token);

                        return Ok(new { token = jwt });
                    }
                    else
                    {
                        return Unauthorized("Email or password is incorrect.");
                    }
                }
            }
            catch (SqlException ex)
            {
                return BadRequest($"SQL error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
