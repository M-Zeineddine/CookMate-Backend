﻿using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Data.Interfaces
{
    public interface IRecipeRepository
    {
        Task<ResponseResult<bool>> AddReviewAsync(CreateReviewModel reviewDto);
        Task<ResponseResult<ReviewAggregateModel>> GetReviewsForRecipeAsync(int recipeId, int pageNumber, int pageSize);
        Task<ResponseResult<bool>> RemoveReviewAsync(int reviewId, int userId);
        Task<ResponseResult<bool>> AddRecipeViewAsync(CreateRecipeViewModel viewModel);
        Task<ResponseResult<List<RecipeDto>>> GetTopRatedRecipesAsync(int count);
        Task<ResponseResult<List<RecentViewDto>>> GetRecentViewsByUserAsync(int userId);
    }
}