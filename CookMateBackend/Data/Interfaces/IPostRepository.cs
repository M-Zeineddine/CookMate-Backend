using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IPostRepository
    {
        Task<ResponseResult<List<UserPostsModel>>> GetPostsByUserId(int userId);
        Task<ResponseResult<Post>> CreatePost(CreatePostModel model);
        Task<RecipeDetailsModel> GetRecipeDetailsByIdAsync(int recipeId);
        Task<ResponseResult<List<RecipeDto>>> GetRecipeFeedForUserAsync(int loggedInUserId);
        Task<ResponseResult<List<MediaDto>>> GetMediaFeedForUserAsync(int loggedInUserId);
        Task<bool> UpdateMediaLikesAsync(int mediaId, bool addLike);
        Task<ResponseResult<List<RecipeSearchResultModel>>> SearchRecipesAsync(string searchString);


    }
}
