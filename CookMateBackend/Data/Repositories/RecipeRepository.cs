﻿using CookMateBackend.Data.Interfaces;
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


        public async Task<ResponseResult<List<RecentViewDto>>> GetRecentViewsByUserAsync(int userId)
        {
            string recipeMediaPath = "uploads/recipes/";

            var result = new ResponseResult<List<RecentViewDto>>();
            try
            {
                var recentViews = await _context.InteractionHistories
                    .Where(ih => ih.UserId == userId)
                    .Join(_context.RecipeViews, ih => ih.InteractionId, rv => rv.InteractionId, (ih, rv) => new { ih, rv })
                    .Join(_context.Recipes, combined => combined.rv.RecipeId, r => r.Id, (combined, r) => new { combined.ih, combined.rv, r })
                    .OrderByDescending(combined => combined.rv.ViewedAt)
                    .Select(combined => new RecentViewDto
                    {
                        RecipeId = combined.r.Id,
                        RecipeName = combined.r.Name,
                        ViewedAt = (DateTime)combined.rv.ViewedAt,
                        Media = combined.r.Media != null ? $"{baseUrl}{recipeMediaPath}{combined.r.Media}" : null,
                        PreparationTime = combined.r.PreperationTime,
                        AverageRating = combined.r.Reviews.Any() ? combined.r.Reviews.Average(r => r.Rating) : 0 // Calculate average rating
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

    }
}