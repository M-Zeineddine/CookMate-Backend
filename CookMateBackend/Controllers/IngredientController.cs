using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        public readonly CookMateContext _context;
        private readonly IIngredientRepository _ingredientRepository;


        public IngredientController(CookMateContext cookMateContext, IIngredientRepository ingredientRepository)
        {
            _context = cookMateContext;
            _ingredientRepository = ingredientRepository;
        }

        [HttpPost("add-ingredient")]
        public async Task<ResponseResult<Ingredient>> AddIngredient([FromForm] CreateIngredientModel model)
        {
            var result = new ResponseResult<Ingredient>();

            // Validate the input data
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                result.IsSuccess = false;
                result.Message = "Missing input: Ingredient Name is required.";
                return result;
            }

            // Check for duplicate ingredient name
            bool ingredientExists = _context.Ingredients.Any(i => i.Name.ToLower() == model.Name.ToLower());
            if (ingredientExists)
            {
                result.IsSuccess = false;
                result.Message = "An ingredient with this name already exists.";
                return result;
            }

            string uniqueFileName = null;
            if (model.Image != null)
            {
                uniqueFileName = await _ingredientRepository.SaveImage(model.Image);
            }

            var newIngredient = new Ingredient
            {
                Name = model.Name,
                Media = uniqueFileName // The file path to the image
            };

            try
            {
                _context.Ingredients.Add(newIngredient);
                await _context.SaveChangesAsync();

                result.IsSuccess = true;
                result.Result = newIngredient;
                result.Message = "Ingredient added successfully.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"An error occurred while adding the ingredient: {ex.Message}";
            }

            return result;
        }


        [HttpGet]
        [Route("searchIngredients")]
        public async Task<ActionResult<ResponseResult<List<IngredientSearchResultModel>>>> SearchIngredients([FromQuery] string searchString = "")
        {
            ResponseResult<List<IngredientSearchResultModel>> responseResult = await _ingredientRepository.SearchIngredientsAsync(searchString);
            return responseResult;
        }

        [HttpPost]
        [Route("addRecipeIngredients")]
        public async Task<ResponseResult<List<RecipeIngredient>>> AddIngredients([FromBody] List<IngredientModel> ingredientModels)
        {
            // Directly return the result of the AddIngredients method
            return await _ingredientRepository.AddIngredients(ingredientModels);
        }


        [HttpGet("substitutes")]
        public async Task<ActionResult<ResponseResult<List<SubstituteDto>>>> GetSubstitutes(int ingredientId)
        {
            return await _ingredientRepository.GetSubstitutesAsync(ingredientId);
        }

        /*[HttpPost("{ingredientId}/substitutes")]
        public async Task<ActionResult<ResponseResult<SubstituteDto>>> AddSubstitute(int ingredientId, int substituteId)
        {
            return await _ingredientRepository.AddSubstituteAsync(ingredientId, substituteId);
        }*/


    }
}
