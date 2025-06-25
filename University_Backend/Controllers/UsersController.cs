using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace University_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                var users = new List<Dictionary<string, object>>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Users", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            user[reader.GetName(i)] = reader.GetValue(i);
                        }
                        users.Add(user);
                    }
                    reader.Close();
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] Dictionary<string, object> userData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        INSERT INTO Users (
                            UserName, UserSurname, UserRoleCode, UserEmail, UserPassword,
                            UserNo, UserRegistrationDate, UserBirthDate, UserSex, UserCountry,
                            UserCity, UserAddress, UserZipCode, UserDepartmentCode
                        )
                        VALUES (
                            @UserName, @UserSurname, @UserRoleCode, @UserEmail, @UserPassword,
                            @UserNo, @UserRegistrationDate, @UserBirthDate, @UserSex, @UserCountry,
                            @UserCity, @UserAddress, @UserZipCode, @UserDepartmentCode
                        )", connection);

                    command.Parameters.AddWithValue("@UserName", userData["UserName"]);
                    command.Parameters.AddWithValue("@UserSurname", userData["UserSurname"]);
                    command.Parameters.AddWithValue("@UserRoleCode", userData["UserRoleCode"]);
                    command.Parameters.AddWithValue("@UserEmail", userData["UserEmail"]);
                    command.Parameters.AddWithValue("@UserPassword", userData["UserPassword"]);
                    command.Parameters.AddWithValue("@UserNo", userData["UserNo"]);
                    command.Parameters.AddWithValue("@UserRegistrationDate", Convert.ToDateTime(userData["UserRegistrationDate"]));
                    command.Parameters.AddWithValue("@UserBirthDate", userData["UserBirthDate"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserSex", userData["UserSex"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserCountry", userData["UserCountry"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserCity", userData["UserCity"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserAddress", userData["UserAddress"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserZipCode", userData["UserZipCode"] ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UserDepartmentCode", userData["UserDepartmentCode"] ?? (object)DBNull.Value);

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("User created.") : StatusCode(500, "Insert failed.");
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

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] JsonElement userData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        UPDATE Users SET
                            UserName = @UserName,
                            UserSurname = @UserSurname,
                            UserRoleCode = @UserRoleCode,
                            UserEmail = @UserEmail,
                            UserPassword = @UserPassword,
                            UserNo = @UserNo,
                            UserBirthDate = @UserBirthDate,
                            UserSex = @UserSex,
                            UserCountry = @UserCountry,
                            UserCity = @UserCity,
                            UserAddress = @UserAddress,
                            UserZipCode = @UserZipCode,
                            UserDepartmentCode = @UserDepartmentCode
                        WHERE UserId = @UserId", connection);

                    command.Parameters.AddWithValue("@UserId", id);
                    command.Parameters.AddWithValue("@UserName", GetString(userData, "UserName"));
                    command.Parameters.AddWithValue("@UserSurname", GetString(userData, "UserSurname"));
                    command.Parameters.AddWithValue("@UserRoleCode", GetString(userData, "UserRoleCode"));
                    command.Parameters.AddWithValue("@UserEmail", GetString(userData, "UserEmail"));
                    command.Parameters.AddWithValue("@UserPassword", GetString(userData, "UserPassword"));
                    command.Parameters.AddWithValue("@UserNo", GetString(userData, "UserNo"));
                    command.Parameters.AddWithValue("@UserBirthDate", GetNullableDate(userData, "UserBirthDate"));
                    command.Parameters.AddWithValue("@UserSex", GetNullableString(userData, "UserSex"));
                    command.Parameters.AddWithValue("@UserCountry", GetNullableString(userData, "UserCountry"));
                    command.Parameters.AddWithValue("@UserCity", GetNullableString(userData, "UserCity"));
                    command.Parameters.AddWithValue("@UserAddress", GetNullableString(userData, "UserAddress"));
                    command.Parameters.AddWithValue("@UserZipCode", GetNullableString(userData, "UserZipCode"));
                    command.Parameters.AddWithValue("@UserDepartmentCode", GetNullableString(userData, "UserDepartmentCode"));

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("User updated.") : NotFound();
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

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId", connection);
                    command.Parameters.AddWithValue("@UserId", id);
                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("User deleted.") : NotFound();
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

        // Yardımcı metotlar
        private static string GetString(JsonElement root, string key) =>
            root.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String
                ? val.GetString()
                : "";

        private static object GetNullableString(JsonElement root, string key) =>
            root.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(val.GetString())
                ? val.GetString()
                : DBNull.Value;

        private static object GetNullableDate(JsonElement root, string key) =>
            root.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String && DateTime.TryParse(val.GetString(), out var date)
                ? date
                : DBNull.Value;
    }
}
