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
        private readonly IWebHostEnvironment _hostEnvironment; // To save the file on the server

        public IngredientRepository(CookMateContext cookMateContext, IWebHostEnvironment hostEnvironment)
        {
            _CookMateContext = cookMateContext;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile));
            }

            string uploadsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "ingredients");
            Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName; // Return the file name (you might need to adjust this depending on how you want to store/access the file)
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
