using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;

namespace CookMateBackend.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<ResponseResult<UserDetailsModel>> GetUserById(int userId);
        Task<List<SearchHistory>> GetSearchHistoryByUserIdAsync(int userId);
    }

}
