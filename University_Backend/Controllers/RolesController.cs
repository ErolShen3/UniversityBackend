using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace University_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly string _connectionString;

        public RolesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = new List<Dictionary<string, object>> ();
                using(var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Roles", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        var role = new Dictionary<string, object>();
                        for (int i = 0; i<reader.FieldCount; i++)
                        {
                            role[reader.GetName(i)] = reader.GetValue(i);
                        }
                        roles.Add(role);
                    }
                }
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetRole(int id)
        {
            try
            {
                Dictionary<string, object> role = null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Roles WHERE RoleId = @RoleId", connection);
                    command.Parameters.AddWithValue("@RoleId", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        role = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            role[reader.GetName(i)] = reader.GetValue(i);
                        }
                    }
                }
                if (role == null) return NotFound();
                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateRole([FromBody] Dictionary<string, object> roleData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO Roles (RoleName, RoleCode) VALUES (@RoleName, @RoleCode)", connection);
                    command.Parameters.AddWithValue("@RoleName", roleData["RoleName"].ToString());
                    command.Parameters.AddWithValue("@RoleCode", roleData["RoleCode"].ToString());

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Role created.") : StatusCode(500, "Insert failed.");
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
        public IActionResult UpdateRole(int id, [FromBody] Dictionary<string, object> roleData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Roles SET RoleName = @RoleName, RoleCode = @RoleCode WHERE RoleId = @RoleId", connection);
                    command.Parameters.AddWithValue("@RoleId", id);
                    command.Parameters.AddWithValue("@RoleName", roleData["RoleName"].ToString());
                    command.Parameters.AddWithValue("@RoleCode", roleData["RoleCode"].ToString());

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Role updated.") : NotFound();
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
        public IActionResult DeleteRole(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM Roles WHERE RoleId = @RoleId", connection);
                    command.Parameters.AddWithValue("@RoleId", id);
                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Role deleted.") : NotFound();
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
