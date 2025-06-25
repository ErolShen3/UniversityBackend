using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;

namespace University_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentClassesController : ControllerBase
    {
        private readonly string _connectionString;

        public StudentClassesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("available")]
        public IActionResult GetAvailableClasses([FromQuery] string departmentCode, [FromQuery] string studentNo)
        {
            try
            {
                var classList = new List<Dictionary<string, object>>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        SELECT * FROM Classes
                        WHERE ClassDepartmentCode = @DepartmentCode
                        AND ClassCode NOT IN (
                            SELECT ClassEnrollmentClassCode FROM ClassEnrollments
                            WHERE ClassEnrollmentStudentNo = @StudentNo
                        )
                        AND ClassCapacity > (
                            SELECT COUNT(*) FROM ClassEnrollments 
                            WHERE ClassEnrollmentClassCode = Classes.ClassCode
                        )", connection);

                    command.Parameters.AddWithValue("@DepartmentCode", departmentCode);
                    command.Parameters.AddWithValue("@StudentNo", studentNo);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var cls = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            cls[reader.GetName(i)] = reader.GetValue(i);
                        }
                        classList.Add(cls);
                    }
                }

                return Ok(classList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("enroll")]
        public IActionResult EnrollInClass([FromBody] Dictionary<string, object> data)
        {
            try
            {
                string classCode = data["ClassCode"].ToString();
                string studentNo = data["StudentNo"].ToString();

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Aynı dersi tekrar alma kontrolü
                    var checkCommand = new SqlCommand(@"
                        SELECT COUNT(*) FROM ClassEnrollments 
                        WHERE ClassEnrollmentClassCode = @ClassCode AND ClassEnrollmentStudentNo = @StudentNo", connection);
                    checkCommand.Parameters.AddWithValue("@ClassCode", classCode);
                    checkCommand.Parameters.AddWithValue("@StudentNo", studentNo);

                    int existing = (int)checkCommand.ExecuteScalar();
                    if (existing > 0)
                        return BadRequest("Bu dersi zaten aldınız.");

                    // Kontenjan kontrolü
                    var capacityCommand = new SqlCommand(@"
                        SELECT ClassCapacity FROM Classes WHERE ClassCode = @ClassCode", connection);
                    capacityCommand.Parameters.AddWithValue("@ClassCode", classCode);

                    int capacity = (int)capacityCommand.ExecuteScalar();

                    var countCommand = new SqlCommand(@"
                        SELECT COUNT(*) FROM ClassEnrollments WHERE ClassEnrollmentClassCode = @ClassCode", connection);
                    countCommand.Parameters.AddWithValue("@ClassCode", classCode);

                    int enrolled = (int)countCommand.ExecuteScalar();

                    if (enrolled >= capacity)
                        return BadRequest("Dersin kontenjanı dolmuştur.");

                    // Kayıt işlemi
                    var enrollCommand = new SqlCommand(@"
                        INSERT INTO ClassEnrollments (ClassEnrollmentClassCode, ClassEnrollmentStudentNo, ClassEnrollmentDate)
                        VALUES (@ClassCode, @StudentNo, GETDATE())", connection);
                    enrollCommand.Parameters.AddWithValue("@ClassCode", classCode);
                    enrollCommand.Parameters.AddWithValue("@StudentNo", studentNo);

                    int rows = enrollCommand.ExecuteNonQuery();

                    return rows > 0 ? Ok("Ders başarıyla alındı.") : StatusCode(500, "Kayıt başarısız.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        [HttpGet("myclasses")]
        public IActionResult GetMyClasses([FromQuery] string studentNo)
        {
            try
            {
                var result = new List<Dictionary<string, object>>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        SELECT c.*, e.ClassEnrollmentId, e.ClassEnrollmentMidtermGrade, e.ClassEnrollmentFinalGrade
                        FROM ClassEnrollments e
                        JOIN Classes c ON c.ClassCode = e.ClassEnrollmentClassCode
                        WHERE e.ClassEnrollmentStudentNo = @StudentNo", connection);

                    command.Parameters.AddWithValue("@StudentNo", studentNo);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var cls = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            cls[reader.GetName(i)] = reader.GetValue(i);
                        }
                        result.Add(cls);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("drop/{enrollmentId}")]
        public IActionResult DropClass(int enrollmentId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM ClassEnrollments WHERE ClassEnrollmentId = @Id", connection);
                    command.Parameters.AddWithValue("@Id", enrollmentId);
                    connection.Open();

                    int rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Ders bırakıldı.") : NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }
    }
}
