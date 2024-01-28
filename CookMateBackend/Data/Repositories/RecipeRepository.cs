using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
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

    }
}
