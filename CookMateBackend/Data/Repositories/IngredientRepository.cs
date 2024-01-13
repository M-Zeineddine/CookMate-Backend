using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.InputModels;

namespace CookMateBackend.Data.Repositories
{
    public class IngredientRepository: IIngredientRepository
    {
        public readonly CookMateContext _CookMateContext;

        public IngredientRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }

        public async Task<ResponseResult<List<IngredientSearchResultModel>>> SearchIngredientsAsync(string searchString)
        {
            List<IngredientSearchResultModel> searchResults;

            if (string.IsNullOrEmpty(searchString))
            {
                // If the search string is null or empty, return all Ingredient
                searchResults = await _CookMateContext.Ingredients
                    .Select(i => new IngredientSearchResultModel
                    {
                        IngredientId = i.Id,
                        IngredientName = i.Name,
                        IngredientImgUrl = i.Media// Assuming one owner per Ingredient
                    }).ToListAsync();
            }
            else
            {
                // If there's a search string, return Ingredient that match the condition
                searchResults = await _CookMateContext.Ingredients
                    .Where(r => r.Name.Contains(searchString)) // Modify this condition as needed
                    .Select(r => new IngredientSearchResultModel
                    {
                        IngredientId = r.Id,
                        IngredientName = r.Name,
                        IngredientImgUrl = r.Media,
                    }).ToListAsync();
            }

            // Wrap in a ResponseResult and return
            return new ResponseResult<List<IngredientSearchResultModel>>
            {
                IsSuccess = searchResults.Any(),
                Message = searchResults.Any() ? "Ingredients found." : "No Ingredients found.",
                Result = searchResults
            };
        }


        public async Task<ResponseResult<List<RecipeIngredient>>> AddIngredients(IEnumerable<IngredientModel> ingredientModels)
        {
            var result = new ResponseResult<List<RecipeIngredient>>();

            try
            {
                List<RecipeIngredient> newIngredients = new List<RecipeIngredient>();

                foreach (var ingredientModel in ingredientModels)
                {
                    RecipeIngredient newIngredient = new RecipeIngredient
                    {
                        Weight = ingredientModel.Weight,
                        RecipeId = ingredientModel.RecipeId,
                        IngredientListId = ingredientModel.IngredientListId
                    };

                    newIngredients.Add(newIngredient);
                    _CookMateContext.RecipeIngredients.Add(newIngredient);
                }

                await _CookMateContext.SaveChangesAsync();

                result.IsSuccess = true;
                result.Result = newIngredients;
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
