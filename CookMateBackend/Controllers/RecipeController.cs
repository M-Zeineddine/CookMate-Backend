using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.Record.CF;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : Controller
    {
        private readonly IRecipeRepository _recipeRepository;
        public readonly CookMateContext _context;
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";


        // Assume these are properly initialized and updated elsewhere
        private Dictionary<int, Dictionary<int, float>> userItemMatrix = new Dictionary<int, Dictionary<int, float>>();
        private Dictionary<int, Dictionary<int, float>> userSimilarities = new Dictionary<int, Dictionary<int, float>>();
        public RecipeController(IRecipeRepository recipeRepository, CookMateContext cookMateContext)
        {
            _context = cookMateContext;
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
        public async Task<ResponseResult<ReviewAggregateModel>> GetReviewsForRecipe(int recipeId, int pageNumber, int pageSize, int userId)
        {
            return await _recipeRepository.GetReviewsForRecipeAsync(recipeId, pageNumber, pageSize, userId);
        }

        [HttpPost]
        [Route("removeReview")]
        public async Task<ResponseResult<bool>> RemoveReview(DeleteReviewModel model)
        {
            return await _recipeRepository.DeleteReviewAsync(model);
        }

        [HttpPost]
        [Route("addView")]
        public async Task<ResponseResult<bool>> AddView([FromBody] CreateRecipeViewModel viewModel)
        {
            return await _recipeRepository.AddRecipeViewAsync(viewModel);
        }





        [HttpGet]
        [Route("recommended")]
        public ResponseResult<IEnumerable<RecipeDto>> GetUserGeneratedContent(int userId)
        {
            string recipeMediaPath = "uploads/recipes/";

            var likedRecipeIds = _context.RecipeLikes
                .Where(rl => rl.Interaction.UserId == userId)
                .Select(rl => rl.RecipeId)
                .Distinct()
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            var viewedRecipeIds = _context.RecipeViews
                .Where(rv => rv.Interaction.UserId == userId)
                .Select(rv => rv.RecipeId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            // After getting liked and viewed recipe IDs
            var preferredTagIds = _context.UserPreferencesTags
                .Where(upt => upt.UserId == userId)
                .Select(upt => upt.TagId)
                .ToList();

            var searchedRecipeIds = _context.SearchHistories
                .Where(sh => sh.UserId == userId)
                .SelectMany(sh => _context.Recipes
                    .Where(r => r.Name.Contains(sh.SearchTerm))
                    .Select(r => r.Id))
                .ToList();

            // Find recipes that match the user's preferred tags
            var taggedRecipeIds = _context.RecipeTags
                .Where(rt => preferredTagIds.Contains(rt.TagListId))
                .Select(rt => rt.RecipeId)
                .ToList();



            // Find similar recipes based on liked and viewed recipe tags
            var similarToLikedRecipes = FindSimilarRecipesByTags(likedRecipeIds);
            var similarToViewedRecipes = FindSimilarRecipesByTags(viewedRecipeIds);


            // Generate recommendations based on collaborative filtering
            var recommendedRecipeIds = GetRecommendations(userId);

            // Combine all recommended IDs including tagged recipes
            var allRecommendedIds = similarToLikedRecipes
                .Union(similarToViewedRecipes)
                .Union(recommendedRecipeIds)
                .Union(taggedRecipeIds) // Include tagged recipes
                .Union(searchedRecipeIds) // Include recipes from search history
                .Distinct()
                .ToList();

            // Retrieve and prepare the recipes for return
            var recipes = _context.Recipes
                .Where(r => allRecommendedIds.Contains(r.Id))
                .Include(r => r.Reviews) // Eagerly load Reviews
                .ToList();

            var recipeDtos = recipes.Select(recipe => new RecipeDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                PreparationTime = recipe.PreperationTime,
                Media = !string.IsNullOrEmpty(recipe.Media) ? $"{baseUrl}{recipeMediaPath}{recipe.Media}" : null,
                AverageRating = recipe.Reviews.Any() ? recipe.Reviews.Average(r => r.Rating) : 0, // Calculate average rating
                CreatedAt = recipe.CreatedAt,
                Score = CalculateRecipeScore(
                    recipe.Id,
                    likedRecipeIds,
                    viewedRecipeIds, userId
                    ) // Pass additional parameters
            });

            var sortedRecipes = recipeDtos.OrderByDescending(r => r.Score).ToList();

            // Wrap the sortedRecipes in a ResponseResult object
            var result = new ResponseResult<IEnumerable<RecipeDto>>()
            {
                IsSuccess = true,
                Message = "Recipes fetched successfully",
                Result = sortedRecipes
            };
            return result;
        }

        private int CalculateRecipeScore(int recipeId, IEnumerable<int> likedRecipeIds, IEnumerable<int> viewedRecipeIds, int userId)
        {
            const int likeWeight = 10;
            const int viewWeight = 1;
            const int preferenceTagWeight = 5;
            const int searchHistoryWeight = 3;

            int likesScore = likedRecipeIds.Count(id => id == recipeId) * likeWeight;
            int viewsScore = viewedRecipeIds.Count(id => id == recipeId) * viewWeight;

            // Get the recipe object once and use it for both name and description checks
            var recipe = _context.Recipes
        .AsNoTracking()
        .FirstOrDefault(r => r.Id == recipeId);

            // Return 0 score if recipe is not found
            if (recipe == null) return 0;

            // Get user's preferred tags in-memory
            var preferredTagIds = _context.UserPreferencesTags
                .AsNoTracking()
                .Where(upt => upt.UserId == userId)
                .Select(upt => upt.TagId)
                .ToList();

            // Get user's search history in-memory
            var searchTerms = _context.SearchHistories
                   .AsNoTracking()
                   .Where(sh => sh.UserId == userId && !string.IsNullOrEmpty(sh.SearchTerm))
                   .Select(sh => sh.SearchTerm)
                   .ToList();

            // Calculate preference tags score based on preferred tags
            int preferenceTagsScore = _context.RecipeTags
                .Where(rt => rt.RecipeId == recipeId)
                .Select(rt => rt.TagListId)
                .ToList() // Bringing this into memory to avoid EF Core translation issues
                .Count(tagId => preferredTagIds.Contains(tagId)) * preferenceTagWeight;

            // Calculate search history score based on search terms
            int searchHistoryScore = searchTerms
                .Where(term => !string.IsNullOrEmpty(recipe.Name) && recipe.Name.Contains(term) ||
                               !string.IsNullOrEmpty(recipe.Description) && recipe.Description.Contains(term))
                .Count() * searchHistoryWeight;


            return likesScore + viewsScore + preferenceTagsScore + searchHistoryScore;
        }

        private List<int> FindSimilarRecipesByTags(IEnumerable<int> recipeIds)
        {
            var tagIds = _context.RecipeTags
                .Where(rt => recipeIds.Contains(rt.RecipeId))
                .Select(rt => rt.TagListId)
                .Distinct()
                .ToList();

            var similarRecipeIds = _context.RecipeTags
                .Where(rt => tagIds.Contains(rt.TagListId) && !recipeIds.Contains(rt.RecipeId))
                .Select(rt => rt.RecipeId)
                .Distinct()
                .ToList();

            return similarRecipeIds;
        }


        private List<int> GetRecommendations(int userId, int N = 5, float Threshold = 0.1f)
        {
            List<int> recommendations = new List<int>();

            if (!userSimilarities.ContainsKey(userId)) return recommendations;

            var similarUsers = userSimilarities[userId].OrderByDescending(kv => kv.Value).Take(N).Select(kv => kv.Key);

            foreach (var similarUser in similarUsers)
            {
                if (!userItemMatrix.ContainsKey(similarUser)) continue;

                var similarUserLikedRecipes = userItemMatrix[similarUser]
                    .Where(kv => kv.Value > Threshold)
                    .Select(kv => kv.Key);

                foreach (var recipeId in similarUserLikedRecipes)
                {
                    if (!userItemMatrix.ContainsKey(userId) || !userItemMatrix[userId].ContainsKey(recipeId))
                    {
                        recommendations.Add(recipeId);
                    }
                }
            }

            return recommendations.Distinct().ToList();
        }






        [HttpGet]
        [Route("topRated")]
        public async Task<ResponseResult<List<RecipeDto>>> GetTopRatedRecipes([FromQuery] int count = 10)
        {
            return await _recipeRepository.GetTopRatedRecipesAsync(count);
        }

        [HttpGet]
        [Route("topFavorited")]
        public async Task<ResponseResult<List<RecipeDto>>> GetTopFavoritedRecipes([FromQuery] int count = 10)
        {
            return await _recipeRepository.GetTopFavoritedRecipesAsync(count);
        }
    
        [HttpPost]
        [Route("searchRecipesByIngredients")]
        public async Task<ResponseResult<List<RecipeDto>>> SearchRecipesByIngredients(List<int> ingredientIds)
        {
            return await _recipeRepository.SearchRecipesByIngredientsAsync(ingredientIds);
        }

        [HttpGet]
        [Route("recentViews")]
        public async Task<ResponseResult<List<RecentViewDto>>> GetRecentViews(int userId)
        {
            return await _recipeRepository.GetRecentViewsByUserAsync(userId);
        }

        [HttpPost]
        [Route("deleteRecipe")]
        public async Task<ResponseResult<bool>> SoftDeleteRecipe(int id)
        {
            return await _recipeRepository.SoftDeleteRecipeAsync(id);
        }

    }
}
