using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [Route("searchIngredients")]
        public async Task<ActionResult<ResponseResult<List<IngredientSearchResultModel>>>> SearchIngredients([FromQuery] string searchString = "")
        {
            ResponseResult<List<IngredientSearchResultModel>> responseResult = await _ingredientRepository.SearchIngredientsAsync(searchString);
            return responseResult;
        }

        [HttpPost]
        [Route("addIngredients")]
        public async Task<ResponseResult<List<RecipeIngredient>>> AddIngredients(IEnumerable<IngredientModel> ingredientModels)
        {
            // Directly return the result of the AddIngredients method
            return await _ingredientRepository.AddIngredients(ingredientModels);
        }


    }
}
