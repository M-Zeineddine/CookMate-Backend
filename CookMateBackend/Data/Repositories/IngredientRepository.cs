using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.InputModels;

namespace CookMateBackend.Data.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        public readonly CookMateContext _CookMateContext;
        private readonly IWebHostEnvironment _hostEnvironment; // To save the file on the server
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";

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
            string ingredientMediaPath = "uploads/ingredients/";

            if (string.IsNullOrEmpty(searchString))
            {
                // If the search string is null or empty, return all Ingredient
                searchResults = await _CookMateContext.Ingredients
                    .Select(i => new IngredientSearchResultModel
                    {
                        IngredientId = i.Id,
                        IngredientName = i.Name,
                        IngredientImgUrl = i.Media != null ? $"{baseUrl}{ingredientMediaPath}{i.Media}" : null,
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
                        IngredientImgUrl = r.Media != null ? $"{baseUrl}{ingredientMediaPath}{r.Media}" : null,
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
                result.Message = "ingredients added!";
                result.Result = newIngredients;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }


        public async Task<ResponseResult<List<SubstituteDto>>> GetSubstitutesAsync(int ingredientId)
        {
            string ingredientsMediaPath = "uploads/ingredients/";

            var substitutes = await _CookMateContext.IngredientSubstitutes
                .Where(sub => sub.IngredientId == ingredientId)
                .Select(sub => new SubstituteDto
                {
                    Id = sub.Substitute.Id,
                    Name = sub.Substitute.Name,
                    MediaUrl = !string.IsNullOrEmpty(sub.Substitute.Media) ? $"{baseUrl}{ingredientsMediaPath}{sub.Substitute.Media}" : null
                })
                .ToListAsync();

            return new ResponseResult<List<SubstituteDto>>
            {
                IsSuccess = substitutes.Any(),
                Message = substitutes.Any() ? null : "No substitutes found.",
                Result = substitutes
            };
        }


       /* public async Task<ResponseResult<SubstituteDto>> AddSubstituteAsync(int ingredientId, int substituteId)
        {
            var result = new ResponseResult<SubstituteDto>();

            // Check if the main ingredient exists
            var ingredientExists = await _CookMateContext.Ingredients.AnyAsync(i => i.Id == ingredientId);
            if (!ingredientExists)
            {
                result.IsSuccess = false;
                result.Message = "Main ingredient not found.";
                return result;
            }

            // Check if the substitute ingredient exists
            var substituteExists = await _CookMateContext.Ingredients.AnyAsync(i => i.Id == substituteId);
            if (!substituteExists)
            {
                result.IsSuccess = false;
                result.Message = "Substitute ingredient not found.";
                return result;
            }

            // Check if the substitute relationship already exists
            var substituteRelationExists = await _CookMateContext.IngredientSubstitutes
                .AnyAsync(s => s.IngredientId == ingredientId && s.SubstituteId == substituteId);
            if (substituteRelationExists)
            {
                result.IsSuccess = false;
                result.Message = "This substitute is already linked to the ingredient.";
                return result;
            }

            var substituteRelation = new IngredientSubstitute
            {
                IngredientId = ingredientId,
                SubstituteId = substituteId
            };

            try
            {
                _CookMateContext.IngredientSubstitutes.Add(substituteRelation);
                await _CookMateContext.SaveChangesAsync();

                var substituteDto = new SubstituteDto
                {
                    Id = substituteId,
                    // Fetch the name and media URL for the substitute
                    Name = _CookMateContext.Ingredients.Where(i => i.Id == substituteId).Select(i => i.Name).FirstOrDefault(),
                    MediaUrl = _CookMateContext.Ingredients.Where(i => i.Id == substituteId).Select(i => i.Media).FirstOrDefault()
                };

                return new ResponseResult<SubstituteDto>
                {
                    IsSuccess = true,
                    Message = "Substitute added successfully.",
                    Result = substituteDto
                };
            }
            catch (Exception ex)
            {
                // Log the exception here if necessary
                return new ResponseResult<SubstituteDto>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while adding the substitute: {ex.Message}"
                    // Do not set the Result in case of an error
                };
            }

        }*/
    }
}
