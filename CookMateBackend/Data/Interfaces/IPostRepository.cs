using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IPostRepository
    {
        Task<ResponseResult<List<UserPostsModel>>> GetPostsByUserId(int userId, int postType);
    }
}
