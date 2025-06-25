using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace UniversityBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassNotesController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _uploadPath;

        public ClassNotesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        [HttpGet]
        public IActionResult GetClassNotes()
        {
            try
            {
                var notes = new List<Dictionary<string, object>>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("SELECT * FROM ClassNotes", connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var note = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            note[reader.GetName(i)] = reader.GetValue(i);
                        }
                        notes.Add(note);
                    }
                }
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadClassNote([FromForm] string ClassNoteClassCode, [FromForm] string ClassNoteName, [FromForm] string ClassNoteUploadDate, [FromForm] IFormFile File)
        {
            try
            {
                if (File == null || File.Length == 0)
                    return BadRequest("No file provided.");

                var fileName = Guid.NewGuid() + Path.GetExtension(File.FileName);
                var relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");
                var fullPath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                DateTime.TryParse(ClassNoteUploadDate, out var uploadDate);

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        INSERT INTO ClassNotes 
                        (ClassNoteClassCode, ClassNoteName, ClassNoteSource, ClassNoteUploadDate)
                        VALUES 
                        (@ClassCode, @NoteName, @Source, @UploadDate)", connection);

                    command.Parameters.AddWithValue("@ClassCode", ClassNoteClassCode);
                    command.Parameters.AddWithValue("@NoteName", ClassNoteName);
                    command.Parameters.AddWithValue("@Source", "/" + relativePath);
                    command.Parameters.AddWithValue("@UploadDate", uploadDate);

                    connection.Open();
                    var rows = await command.ExecuteNonQueryAsync();
                    return rows > 0 ? Ok("Note uploaded.") : StatusCode(500, "Insert failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClassNote(int id, [FromBody] Dictionary<string, object> data)
        {
            try
            {
                DateTime.TryParse(data["ClassNoteUploadDate"]?.ToString(), out var uploadDate);

                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand(@"
                        UPDATE ClassNotes SET 
                            ClassNoteClassCode = @ClassCode,
                            ClassNoteName = @NoteName,
                            ClassNoteSource = @Source,
                            ClassNoteUploadDate = @UploadDate
                        WHERE ClassNoteId = @Id", connection);

                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ClassCode", data["ClassNoteClassCode"].ToString());
                    command.Parameters.AddWithValue("@NoteName", data["ClassNoteName"].ToString());
                    command.Parameters.AddWithValue("@Source", data["ClassNoteSource"].ToString());
                    command.Parameters.AddWithValue("@UploadDate", uploadDate);

                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Note updated.") : NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClassNote(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var command = new SqlCommand("DELETE FROM ClassNotes WHERE ClassNoteId = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    var rows = command.ExecuteNonQuery();
                    return rows > 0 ? Ok("Note deleted.") : NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
