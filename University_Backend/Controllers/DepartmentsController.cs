using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace UniversityBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly string _connectionString;

        public DepartmentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetDepartments()
        {
            try
            {
                var departments = new List<Dictionary<string, object>>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Departments", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var dept = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dept[reader.GetName(i)] = reader.GetValue(i);
                        }
                        departments.Add(dept);
                    }
                }
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetDepartment(int id)
        {
            try
            {
                Dictionary<string, object> dept = null;
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Departments WHERE DepartmentId = @DepartmentId", connection);
                    command.Parameters.AddWithValue("@DepartmentId", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        dept = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            dept[reader.GetName(i)] = reader.GetValue(i);
                        }
                    }
                }
                if (dept == null) return NotFound();
                return Ok(dept);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateDepartment([FromBody] Dictionary<string, object> deptData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("INSERT INTO Departments (DepartmentName, DepartmentCode) VALUES (@DepartmentName, @DepartmentCode)", connection);
                    command.Parameters.AddWithValue("@DepartmentName", deptData["DepartmentName"].ToString());
                    command.Parameters.AddWithValue("@DepartmentCode", deptData["DepartmentCode"].ToString());

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Department created.") : StatusCode(500, "Insert failed.");
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
        public IActionResult UpdateDepartment(int id, [FromBody] Dictionary<string, object> deptData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("UPDATE Departments SET DepartmentName = @DepartmentName, DepartmentCode = @DepartmentCode WHERE DepartmentId = @DepartmentId", connection);
                    command.Parameters.AddWithValue("@DepartmentId", id);
                    command.Parameters.AddWithValue("@DepartmentName", deptData["DepartmentName"].ToString());
                    command.Parameters.AddWithValue("@DepartmentCode", deptData["DepartmentCode"].ToString());

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Department updated.") : NotFound();
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
        public IActionResult DeleteDepartment(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM Departments WHERE DepartmentId = @DepartmentId", connection);
                    command.Parameters.AddWithValue("@DepartmentId", id);
                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Department deleted.") : NotFound();
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
