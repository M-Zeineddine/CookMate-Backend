using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Data.Interfaces
{
    public interface IRecipeRepository
    {
        Task<ResponseResult<bool>> AddReviewAsync(CreateReviewModel reviewDto);
        Task<ResponseResult<ReviewAggregateModel>> GetReviewsForRecipeAsync(int recipeId, int pageNumber, int pageSize, int userId);
        Task<ResponseResult<bool>> DeleteReviewAsync(DeleteReviewModel model);
        Task<ResponseResult<bool>> AddRecipeViewAsync(CreateRecipeViewModel viewModel);
/*        IEnumerable<RecipeDto> GetRecommendedRecipesAsync(int userId);
*/        Task<ResponseResult<List<RecipeDto>>> GetTopRatedRecipesAsync(int count);
        Task<ResponseResult<List<RecipeDto>>> GetTopFavoritedRecipesAsync(int count);
        Task<ResponseResult<List<RecentViewDto>>> GetRecentViewsByUserAsync(int userId);
        Task<ResponseResult<List<RecipeDto>>> SearchRecipesByIngredientsAsync(List<int> ingredientIds);
        Task<ResponseResult<bool>> SoftDeleteRecipeAsync(int recipeId);

    }
}
