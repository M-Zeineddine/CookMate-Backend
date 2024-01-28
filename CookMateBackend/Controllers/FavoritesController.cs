using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using Microsoft.AspNetCore.Mvc;
using CookMateBackend.Data.Repositories;

namespace CookMateBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : Controller
    {
        private readonly CookMateContext _context; // Your DbContext or service that contains the method
        private readonly IFavoritesRepository _favoriteRepository;


        public FavoritesController(CookMateContext context, IFavoritesRepository favoritesRepository)
        {
            _context = context;
            _favoriteRepository = favoritesRepository;
        }

        [HttpPost]
        [Route("toggleFavorite")]
        public async Task<ResponseResult<Favorite>> ToggleFavorite([FromBody] FavoriteModel favoriteModel)
        {
            return await _favoriteRepository.ToggleFavorite(favoriteModel);
        }

        [HttpGet]
        [Route("getFavorites")]
        public async Task<ResponseResult<List<UserPostsModel>>> GetFavorites(int userId, [FromQuery] string favoriteType)
        {
            return await _favoriteRepository.GetFavorites(userId, favoriteType);
        }




    }
}
