using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IIngredientRepository
    {
        Task<ResponseResult<List<IngredientSearchResultModel>>> SearchIngredientsAsync(string searchString);
        Task<ResponseResult<List<RecipeIngredient>>> AddIngredients(IEnumerable<IngredientModel> ingredientModels);
        Task<string> SaveImage(IFormFile imageFile);
    }
}
