using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<ResponseResult<UserDetailsModel>> GetUserDetailsById(int userId);
        Task<ResponseResult<List<UserPostsModel>>> GetUserPosts(int userId, int postType);
        Task<ResponseResult<List<UserMediasModel>>> GetUserMedia(int userId);
    }
}
