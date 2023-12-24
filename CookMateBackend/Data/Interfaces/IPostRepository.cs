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

    }
}
