using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniversityBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly string _connectionString;

        public ClassesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetClasses()
        {
            try
            {
                var classes = new List<Dictionary<string, object>>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM Classes", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var cls = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            cls[reader.GetName(i)] = reader.GetValue(i);
                        }
                        classes.Add(cls);
                    }
                }
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateClass([FromBody] Dictionary<string, object> classData)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand(@"
                    INSERT INTO Classes 
                    (ClassName, ClassCode, ClassTeacherNo, ClassDescription, ClassCapacity, ClassDay, ClassStartTime, ClassEndTime, ClassCredits, ClassDepartmentCode)
                    VALUES 
                    (@ClassName, @ClassCode, @ClassTeacherNo, @ClassDescription, @ClassCapacity, @ClassDay, @ClassStartTime, @ClassEndTime, @ClassCredits, @ClassDepartmentCode)", connection);

                var json = System.Text.Json.JsonSerializer.Serialize(classData);
                var root = System.Text.Json.JsonDocument.Parse(json).RootElement;

                command.Parameters.AddWithValue("@ClassName", root.GetProperty("ClassName").GetString());
                command.Parameters.AddWithValue("@ClassCode", root.GetProperty("ClassCode").GetString());
                command.Parameters.AddWithValue("@ClassTeacherNo", GetNullableString(root, "ClassTeacherNo"));
                command.Parameters.AddWithValue("@ClassDescription", GetNullableString(root, "ClassDescription"));
                command.Parameters.AddWithValue("@ClassDay", GetNullableString(root, "ClassDay"));
                command.Parameters.AddWithValue("@ClassDepartmentCode", GetNullableString(root, "ClassDepartmentCode"));
                command.Parameters.AddWithValue("@ClassCapacity", TryGetInt(root, "ClassCapacity"));
                command.Parameters.AddWithValue("@ClassCredits", TryGetInt(root, "ClassCredits"));
                command.Parameters.AddWithValue("@ClassStartTime", TryGetTimeSpan(root, "ClassStartTime"));
                command.Parameters.AddWithValue("@ClassEndTime", TryGetTimeSpan(root, "ClassEndTime"));

                connection.Open();
                var rows = command.ExecuteNonQuery();
                return rows > 0 ? Ok("Class created.") : StatusCode(500, "Insert failed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClass(int id, [FromBody] Dictionary<string, object> classData)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand(@"
                    UPDATE Classes SET 
                        ClassName = @ClassName,
                        ClassCode = @ClassCode,
                        ClassTeacherNo = @ClassTeacherNo,
                        ClassDescription = @ClassDescription,
                        ClassCapacity = @ClassCapacity,
                        ClassDay = @ClassDay,
                        ClassStartTime = @ClassStartTime,
                        ClassEndTime = @ClassEndTime,
                        ClassCredits = @ClassCredits,
                        ClassDepartmentCode = @ClassDepartmentCode
                    WHERE ClassId = @ClassId", connection);

                var json = System.Text.Json.JsonSerializer.Serialize(classData);
                var root = System.Text.Json.JsonDocument.Parse(json).RootElement;

                command.Parameters.AddWithValue("@ClassId", id);
                command.Parameters.AddWithValue("@ClassName", root.GetProperty("ClassName").GetString());
                command.Parameters.AddWithValue("@ClassCode", root.GetProperty("ClassCode").GetString());
                command.Parameters.AddWithValue("@ClassTeacherNo", GetNullableString(root, "ClassTeacherNo"));
                command.Parameters.AddWithValue("@ClassDescription", GetNullableString(root, "ClassDescription"));
                command.Parameters.AddWithValue("@ClassDay", GetNullableString(root, "ClassDay"));
                command.Parameters.AddWithValue("@ClassDepartmentCode", GetNullableString(root, "ClassDepartmentCode"));
                command.Parameters.AddWithValue("@ClassCapacity", TryGetInt(root, "ClassCapacity"));
                command.Parameters.AddWithValue("@ClassCredits", TryGetInt(root, "ClassCredits"));
                command.Parameters.AddWithValue("@ClassStartTime", TryGetTimeSpan(root, "ClassStartTime"));
                command.Parameters.AddWithValue("@ClassEndTime", TryGetTimeSpan(root, "ClassEndTime"));

                connection.Open();
                var rows = command.ExecuteNonQuery();
                return rows > 0 ? Ok("Class updated.") : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClass(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("DELETE FROM Classes WHERE ClassId = @ClassId", connection);
                command.Parameters.AddWithValue("@ClassId", id);
                connection.Open();
                var rows = command.ExecuteNonQuery();
                return rows > 0 ? Ok("Class deleted.") : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helpers
        private static object GetNullableString(System.Text.Json.JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && !string.IsNullOrWhiteSpace(prop.GetString())
                ? (object)prop.GetString()
                : DBNull.Value;
        }

        private static object TryGetInt(System.Text.Json.JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var prop) && int.TryParse(prop.ToString(), out var result)
                ? result
                : DBNull.Value;
        }

        private static object TryGetTimeSpan(System.Text.Json.JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var prop)) return DBNull.Value;

            var raw = prop.ToString();

            // Önce hh:mm:ss, sonra hh:mm kontrol edilir
            if (TimeSpan.TryParseExact(raw, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var timeFull))
                return timeFull;

            if (TimeSpan.TryParseExact(raw, @"hh\:mm", CultureInfo.InvariantCulture, out var timeShort))
                return timeShort;

            return DBNull.Value;
        }
    }
}
