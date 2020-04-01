using System;
using System.Threading.Tasks;
using APBD5_sol_s18935.Models;
using Microsoft.AspNetCore.Mvc;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlDataReader = Microsoft.Data.SqlClient.SqlDataReader;
using SqlTransaction = Microsoft.Data.SqlClient.SqlTransaction;


namespace APBD5_sol_s18935.Controllers
{
    
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly string _connectionString =
            "Data Source=db-mssql;Initial Catalog=s15383;Integrated Security=True";

     

       

        [HttpPost]
        public async Task<IActionResult> registerStudent(StdEnr input)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand com = new SqlCommand();
            Study study;
            {
                com.Connection = conn;
                com.CommandText = "select IdStudy, Name from Studies where Name = @name";

                com.Parameters.AddWithValue("name", input.Studies);

                conn.Open();

                try
                {
                    SqlDataReader dataReader = await com.ExecuteReaderAsync();
                    await dataReader.ReadAsync();
                    study = new Study
                    {
                        IdStudy = int.Parse(dataReader["IdStudy"].ToString()),
                        Name = dataReader["Name"].ToString()
                    };
                    if (study == null)
                    {
                        return BadRequest();
                    }

                    
                }
                catch
                {
                    return null;
                }
            }
             conn = new SqlConnection(_connectionString);
             com = new SqlCommand();

            conn.Open();
            SqlTransaction transaction = conn.BeginTransaction();

            com.Connection = conn;
            com.Transaction = transaction;
            com.CommandText =
                @"declare @enrollmentId int
                      select
                          @enrollmentId = e.IdEnrollment
                      from Enrollment e
                          left join Studies s on e.IdStudy = s.IdStudy
                      where e.IdStudy = @studyId and e.Semester = 1
                      
                      if @enrollmentId is null
                      begin

                          select @enrollmentId = max(IdEnrollment) + 1 from Enrollment
                          insert into Enrollment values (@enrollmentId, 1, @studyId, getdate());

                      end

                      insert into Student values (@index, @firstName, @lastName, @birthDate, @enrollmentId)";

            com.Parameters.AddWithValue("studyId", study.IdStudy);
            com.Parameters.AddWithValue("index", input.IndexNumber);
            com.Parameters.AddWithValue("firstName", input.FirstName);
            com.Parameters.AddWithValue("lastName", input.LastName);
            com.Parameters.AddWithValue("birthDate", input.BirthDate);

            try
            {
                await com.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
            }
            
            return StatusCode(200);
        }

        [HttpPost("promotions")]
        public async Task<IActionResult> PromoteStudents(EnrStud input)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand com = new SqlCommand();

            com.Connection = conn;
            com.CommandText =
                @"select 
                        e.IdEnrollment, e.Semester, e.StartDate, s.IdStudy, s.Name
                      from Enrollment e
                        left join Studies s on s.IdStudy = e.IdStudy
                      where 
                        s.Name = @studyName
                        and e.Semester = @semester";

            com.Parameters.AddWithValue("studyName", input.Studies);
            com.Parameters.AddWithValue("semester", input.Semester);

            conn.Open();
            SqlDataReader dataReader = await com.ExecuteReaderAsync();
            await dataReader.ReadAsync();
            Enrollment enr =  new Enrollment
            {
                IdEnrollment = int.Parse(dataReader["IdEnrollment"].ToString()),
                Semester = int.Parse(dataReader["Semester"].ToString()),
                StartDate = DateTime.Parse(dataReader["StartDate"].ToString()),
                Study = new Study
                {
                    IdStudy = int.Parse(dataReader["IdStudy"].ToString()),
                    Name = dataReader["Name"].ToString(),
                }
            };
            
           
            if (enr == null)
            {
                return NotFound();
            }

             conn = new SqlConnection(_connectionString);
             com = new SqlCommand();
            
            com.Connection = conn;
            com.CommandType = System.Data.CommandType.StoredProcedure;
            com.CommandText = "PromoteStudents";

            com.Parameters.AddWithValue("studies", input.Studies);
            com.Parameters.AddWithValue("semester", input.Semester);

            conn.Open();
            await com.ExecuteNonQueryAsync();
            
        
            
            
       
            return StatusCode(200);
        }
    }
}