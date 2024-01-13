using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Repositories
{
    public class ProcedureRepository: IProcedureRepository
    {
        public readonly CookMateContext _CookMateContext;

        public ProcedureRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }




        public async Task<ResponseResult<List<Procedure>>> AddProcedures(IEnumerable<ProcedureModel> procedureModels)
        {
            var result = new ResponseResult<List<Procedure>>();

            try
            {
                List<Procedure> newProcedures = new List<Procedure>();

                foreach (var procedureModel in procedureModels)
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

                    newProcedures.Add(newProcedure);
                    _CookMateContext.Procedures.Add(newProcedure);
                }

                await _CookMateContext.SaveChangesAsync();

                result.IsSuccess = true;
                result.Message = "Procedures added successfully";
                result.Result = newProcedures;
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
