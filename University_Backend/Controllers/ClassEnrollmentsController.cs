using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniversityBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassEnrollmentsController : ControllerBase
    {
        private readonly string _connectionString;

        public ClassEnrollmentsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetClassEnrollments()
        {
            try
            {
                var enrollments = new List<Dictionary<string, object>>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM ClassEnrollments", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var enr = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            enr[reader.GetName(i)] = reader.GetValue(i);
                        }
                        enrollments.Add(enr);
                    }
                }
                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult CreateClassEnrollment([FromBody] Dictionary<string, object> data)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        INSERT INTO ClassEnrollments 
                        (ClassEnrollmentClassCode, ClassEnrollmentStudentNo, ClassEnrollmentDate, ClassEnrollmentMidtermGrade, ClassEnrollmentFinalGrade)
                        VALUES 
                        (@ClassCode, @StudentNo, @Date, @Midterm, @Final)", connection);

                    DateTime.TryParse(data["ClassEnrollmentDate"]?.ToString(), out var date);
                    var midterm = Convert.ToDecimal(data["ClassEnrollmentMidtermGrade"]?.ToString(), CultureInfo.InvariantCulture);
                    var final = Convert.ToDecimal(data["ClassEnrollmentFinalGrade"]?.ToString(), CultureInfo.InvariantCulture);

                    command.Parameters.AddWithValue("@ClassCode", data["ClassEnrollmentClassCode"]?.ToString());
                    command.Parameters.AddWithValue("@StudentNo", data["ClassEnrollmentStudentNo"]?.ToString());
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Midterm", midterm);
                    command.Parameters.AddWithValue("@Final", final);

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Enrollment created.") : StatusCode(500, "Insert failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClassEnrollment(int id, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        UPDATE ClassEnrollments SET 
                            ClassEnrollmentClassCode = @ClassCode,
                            ClassEnrollmentStudentNo = @StudentNo,
                            ClassEnrollmentDate = @Date,
                            ClassEnrollmentMidtermGrade = @Midterm,
                            ClassEnrollmentFinalGrade = @Final
                        WHERE ClassEnrollmentId = @Id", connection);

                    DateTime.TryParse(data["ClassEnrollmentDate"]?.ToString(), out var date);
                    var midterm = Convert.ToDecimal(data["ClassEnrollmentMidtermGrade"]?.ToString(), CultureInfo.InvariantCulture);
                    var final = Convert.ToDecimal(data["ClassEnrollmentFinalGrade"]?.ToString(), CultureInfo.InvariantCulture);

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ClassCode", data["ClassEnrollmentClassCode"]?.ToString());
                    command.Parameters.AddWithValue("@StudentNo", data["ClassEnrollmentStudentNo"]?.ToString());
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@Midterm", midterm);
                    command.Parameters.AddWithValue("@Final", final);

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Enrollment updated.") : NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClassEnrollment(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM ClassEnrollments WHERE ClassEnrollmentId = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Enrollment deleted.") : NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
