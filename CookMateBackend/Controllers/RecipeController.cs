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
    public class RecipeController : Controller
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipeController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpPost]
        [Route("addReview")]
        public async Task<ResponseResult<bool>> AddReview([FromBody] CreateReviewModel reviewDto)
        {
            return await _recipeRepository.AddReviewAsync(reviewDto);
        }

        [HttpGet]
        [Route("getReviewsForRecipe")]
        public async Task<ResponseResult<ReviewAggregateModel>> GetReviewsForRecipe(int recipeId, int pageNumber, int pageSize)
        {
            return await _recipeRepository.GetReviewsForRecipeAsync(recipeId, pageNumber, pageSize);
        }

        [HttpDelete]
        [Route("removeReview")]
        public async Task<ResponseResult<bool>> RemoveReview(int reviewId, [FromQuery] int userId)
        {
            return await _recipeRepository.RemoveReviewAsync(reviewId, userId);
        }

    }
}
