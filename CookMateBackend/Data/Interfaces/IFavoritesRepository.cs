using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Data.Interfaces
{
    public interface IFavoritesRepository
    {
        Task<ResponseResult<Favorite>> ToggleFavorite(FavoriteModel favoriteModel);
        Task<ResponseResult<List<UserPostsModel>>> GetFavorites(int userId, string favoriteType);



    }
}
