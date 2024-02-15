using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Data.Repositories
{
    public class RecipeRepository: IRecipeRepository
    {
        public readonly CookMateContext _context;

        public RecipeRepository(CookMateContext cookMateContext)
        {
            _context = cookMateContext;
        }
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";

        public async Task<ResponseResult<bool>> AddReviewAsync(CreateReviewModel reviewDto)
        {
            var result = new ResponseResult<bool>();

            // Check if the user has already reviewed this recipe
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == reviewDto.UserId && r.RecipesId == reviewDto.RecipeId);

            if (existingReview != null)
            {
                // User has already reviewed this recipe
                result.IsSuccess = false;
                result.Message = "You have already reviewed this recipe.";
                result.Result = false;
                return result;
            }

            // Proceed to add the new review
            var review = new Review
            {
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                UserId = reviewDto.UserId,
                RecipesId = reviewDto.RecipeId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            result.IsSuccess = true;
            result.Message = "Review created successfully";
            result.Result = true;

            return result;
        }


        public async Task<ResponseResult<ReviewAggregateModel>> GetReviewsForRecipeAsync(int recipeId, int pageNumber, int pageSize)
        {
            var result = new ResponseResult<ReviewAggregateModel>();

            try
            {
                var query = _context.Reviews
                    .Where(r => r.RecipesId == recipeId)
                    .Include(r => r.User) // Include the User navigation property
                    .OrderByDescending(r => r.CreatedAt);

                var totalReviews = await query.CountAsync();

                var userReviews = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new UserReviewModel
                    {
                        Id = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        User = new UserModel // Map the User entity to UserModel
                        {
                            Id = r.User.Id,
                            Username = r.User.Username,
                            ProfilePic = r.User.ProfilePic // Assuming ProfilePic is the correct field for the user's profile picture
                        }
                    })
                    .ToListAsync();

                var averageRating = totalReviews > 0
                    ? await query.AverageAsync(r => r.Rating)
                    : 0;

                var reviewAggregate = new ReviewAggregateModel
                {
                    AverageRating = (double)averageRating,
                    TotalReviews = totalReviews,
                    UserReviews = userReviews
                };

                result.IsSuccess = true;
                result.Result = reviewAggregate;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }


        public async Task<ResponseResult<bool>> RemoveReviewAsync(int reviewId, int userId)
        {
            var result = new ResponseResult<bool>();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

            if (review == null)
            {
                result.IsSuccess = false;
                result.Message = "Review not found or you do not have permission to delete this review.";
                result.Result = false;
                return result;
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            result.IsSuccess = true;
            result.Message = "Review removed successfully.";
            result.Result = true;

            return result;
        }


        public async Task<ResponseResult<bool>> AddRecipeViewAsync(CreateRecipeViewModel viewModel)
        {
            var result = new ResponseResult<bool>();

            // First, check if the recipe's corresponding post belongs to the user
            var isUserRecipe = await _context.Posts
                .AnyAsync(p => p.RecipeId == viewModel.recipeId && p.UserId == viewModel.userId && p.Type == 1);
            if (isUserRecipe)
            {
                result.IsSuccess = true;
                result.Result = false; // The user is viewing their own recipe
                result.Message = "User's own recipe view is not counted.";
                return result;
            }

            // Check if the user has viewed the recipe in the last 30 seconds
            var recentView = await _context.RecipeViews
                .OrderByDescending(rv => rv.ViewedAt)
                .FirstOrDefaultAsync(rv => rv.RecipeId == viewModel.recipeId
                                           && rv.Interaction.UserId == viewModel.userId
                                           && rv.ViewedAt > DateTime.UtcNow.AddSeconds(-30));

            if (recentView != null)
            {
                result.IsSuccess = true;
                result.Result = false; // The view is not added because it's too soon
                result.Message = "This recipe has been viewed recently.";
                return result;
            }

            // Insert a record into interaction_history
            var interaction = new InteractionHistory
            {
                UserId = viewModel.userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.InteractionHistories.Add(interaction);
            await _context.SaveChangesAsync();

            // Now insert a record into recipe_views with the new interaction_id
            var recipeView = new RecipeView
            {
                InteractionId = interaction.InteractionId, // Use the newly created ID
                RecipeId = viewModel.recipeId,
                ViewedAt = DateTime.UtcNow
            };
            _context.RecipeViews.Add(recipeView);
            await _context.SaveChangesAsync();

            result.IsSuccess = true;
            result.Result = true; // The view was successfully added
            result.Message = "Recipe view added successfully.";

            return result;
        }


        public async Task<ResponseResult<List<RecipeDto>>> GetTopRatedRecipesAsync(int count)
        {
            string recipeMediaPath = "uploads/recipes/";

            var result = new ResponseResult<List<RecipeDto>>();
            try
            {
                var topRatedRecipes = await _context.Recipes
                    .Where(r => r.Reviews.Any())  // Ensure that the recipe has at least one review
                    .Select(r => new
                    {
                        Recipe = r,
                        AverageRating = r.Reviews.Average(rv => (double?)rv.Rating),
                        TotalRatings = r.Reviews.Count()
                    })
                    .OrderByDescending(r => r.AverageRating)
                    .ThenByDescending(r => r.TotalRatings) // In case of tie in ratings, sort by the number of ratings
                    .Take(count)
                    .Select(r => new RecipeDto
                    {
                        Id = r.Recipe.Id,
                        Name = r.Recipe.Name,
                        Description = r.Recipe.Description,
                        Media = r.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{r.Recipe.Media}" : null,
                        PreparationTime = r.Recipe.PreperationTime,
                        AverageRating = r.Recipe.Reviews.Any() ? r.Recipe.Reviews.Average(r => r.Rating) : 0 // Calculate average rating
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Top rated recipes fetched";
                result.Result = topRatedRecipes;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }


        public async Task<ResponseResult<List<RecipeDto>>> GetTopFavoritedRecipesAsync(int count)
        {
            string recipeMediaPath = "uploads/recipes/";

            var result = new ResponseResult<List<RecipeDto>>();
            try
            {
                var topFavoritedRecipes = await _context.Posts
                    .Where(p => p.Type == 1) // Filter for posts that are recipes
                    .GroupJoin(_context.Favorites, // Join with favorites
                        post => post.Id,
                        favorite => favorite.PostId,
                        (post, favorites) => new { Post = post, FavoritesCount = favorites.Count() })
                    .OrderByDescending(pf => pf.FavoritesCount) // Order by the number of favorites
                    .Take(count)
                    .Select(pf => new RecipeDto
                    {
                        Id = pf.Post.Recipe.Id,
                        Name = pf.Post.Recipe.Name,
                        Description = pf.Post.Recipe.Description,
                        Media = pf.Post.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{pf.Post.Recipe.Media}" : null,
                        PreparationTime = pf.Post.Recipe.PreperationTime,
                        AverageRating = pf.Post.Recipe.Reviews.Any() ? pf.Post.Recipe.Reviews.Average(r => r.Rating) : 0, // Calculate average rating
                                                                                                                          // Add more properties as needed
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Top favorited recipes fetched";
                result.Result = topFavoritedRecipes;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }



        public async Task<ResponseResult<List<RecentViewDto>>> GetRecentViewsByUserAsync(int userId)
        {
            string recipeMediaPath = "uploads/recipes/";

            var result = new ResponseResult<List<RecentViewDto>>();
            try
            {
                // First, get the most recent view date for each recipe
                var mostRecentViewPerRecipe = _context.RecipeViews
                    .Where(rv => rv.Interaction.UserId == userId)
                    .GroupBy(rv => rv.RecipeId)
                    .Select(g => new
                    {
                        RecipeId = g.Key,
                        MostRecentViewAt = g.Max(rv => rv.ViewedAt)
                    });

                var recentViews = await mostRecentViewPerRecipe
                    .Join(_context.Recipes, mr => mr.RecipeId, r => r.Id, (mr, r) => new { mr, r })
                    .OrderByDescending(joined => joined.mr.MostRecentViewAt)
                    .Take(5) // Take the last 5 distinct views
                    .Select(joined => new RecentViewDto
                    {
                        RecipeId = joined.r.Id,
                        RecipeName = joined.r.Name,
                        ViewedAt = (DateTime)joined.mr.MostRecentViewAt,
                        Media = joined.r.Media != null ? $"{baseUrl}{recipeMediaPath}{joined.r.Media}" : null,
                        PreparationTime = joined.r.PreperationTime,
                        AverageRating = joined.r.Reviews.Any() ? joined.r.Reviews.Average(rv => rv.Rating) : 0 // Calculate average rating
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Message = "Recently viewed recipes fetched";
                result.Result = recentViews;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }


        public async Task<ResponseResult<List<RecipeDto>>> SearchRecipesByIngredientsAsync(List<int> ingredientIds)
        {
            string recipeMediaPath = "uploads/recipes/";

            // Return an error response if no ingredient IDs are provided
            if (ingredientIds == null || !ingredientIds.Any())
            {
                return new ResponseResult<List<RecipeDto>>
                {
                    IsSuccess = false,
                    Message = "No ingredient IDs provided.",
                    Result = null
                }; 
            }

            try
            {
                // Find recipes and sort them by the number of matching ingredients in descending order
                var recipes = await _context.Recipes
                                            .Select(r => new
                                            {
                                                Recipe = r,
                                                MatchCount = r.RecipeIngredients.Count(ri => ingredientIds.Contains(ri.IngredientListId))
                                            })
                                            .Where(r => r.MatchCount > 0) // Filter out recipes with no matches
                                            .OrderByDescending(r => r.MatchCount) // Order by match count
                                            .ThenBy(r => r.Recipe.CreatedAt) // Secondary sort if desired
                                            .Select(r => new RecipeDto
                                            {
                                                Id = r.Recipe.Id,
                                                Name = r.Recipe.Name,
                                                Description = r.Recipe.Description,
                                                PreparationTime = r.Recipe.PreperationTime,
                                                Media = r.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{r.Recipe.Media}" : null,
                                                AverageRating = r.Recipe.Reviews.Any() ? r.Recipe.Reviews.Average(review => review.Rating) : 0,
                                                CreatedAt = r.Recipe.CreatedAt,
                                            })
                                            .ToListAsync();

                // Wrap in a ResponseResult and return
                return new ResponseResult<List<RecipeDto>>
                {
                    IsSuccess = recipes.Any(),
                    Message = recipes.Any() ? "Recipes found." : "No recipes found.",
                    Result = recipes
                };
            }
            catch (Exception ex)
            {
                // If there's an exception, return an error response with the exception message
                return new ResponseResult<List<RecipeDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}",
                    Result = null
                };
            }
        }



    }
}
