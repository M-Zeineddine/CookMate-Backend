using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using Microsoft.AspNetCore.Mvc;
using CookMateBackend.Data.Repositories;

namespace CookMateBackend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProcedureController : ControllerBase
    {
        public readonly CookMateContext _context;
        private readonly IProcedureRepository _procedureRepository;


        public ProcedureController(CookMateContext cookMateContext, IProcedureRepository procedureRepository)
        {
            _context = cookMateContext;
            _procedureRepository = procedureRepository;
        }

        [HttpPost]
        [Route("addProcedures")]
        public async Task<ResponseResult<Procedure>> AddProcedure([FromForm] ProcedureModel procedureModel)
        {
            var result = new ResponseResult<Procedure>();

            try
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                string procedureFolderPath = Path.Combine(uploadsFolder, "procedures");
                Directory.CreateDirectory(procedureFolderPath); // Ensure the directory exists

                string? procedureMediaFileName = null;

                if (procedureModel.Media != null)
                {
                    IFormFile procedureFile = procedureModel.Media;
                    string uniqueProcedureFileName = Guid.NewGuid().ToString() + "_" + procedureFile.FileName; // Generate unique file name
                    string procedureFilePath = Path.Combine(procedureFolderPath, uniqueProcedureFileName); // Full path for saving

                    // Save the file
                    using (var stream = new FileStream(procedureFilePath, FileMode.Create))
                    {
                        await procedureFile.CopyToAsync(stream);
                    }

                    procedureMediaFileName = uniqueProcedureFileName; // Store just the file name
                }

                Procedure newProcedure = new Procedure
                {
                    Title = procedureModel.Title,
                    Description = procedureModel.Description,
                    Media = procedureMediaFileName,
                    Time = procedureModel.Time,
                    Step = procedureModel.Step,
                    RecipeId = procedureModel.RecipeId
                };

                _context.Procedures.Add(newProcedure);
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Message = "Procedure added successfully";
                result.Result = newProcedure;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }

    }
}
