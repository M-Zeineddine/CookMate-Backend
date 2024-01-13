using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IProcedureRepository
    {
        Task<ResponseResult<List<Procedure>>> AddProcedures(IEnumerable<ProcedureModel> procedureModels);
    }
}
