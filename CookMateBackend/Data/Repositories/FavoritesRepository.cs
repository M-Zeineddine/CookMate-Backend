using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.OutputModels;

namespace CookMateBackend.Data.Repositories
{
    public class FavoriteRepository : IFavoritesRepository
    {
        private readonly CookMateContext _context;
        public string baseUrl = "http://mz9436-001-site1.ctempurl.com/";

        public FavoriteRepository(CookMateContext context)
        {
            _context = context;
        }

        public async Task<ResponseResult<Favorite>> ToggleFavorite(FavoriteModel favoriteModel)
        {
            var result = new ResponseResult<Favorite>();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    Post? post = null;
                    if (favoriteModel.RecipeId != 0)
                    {
                        post = await _context.Posts.FirstOrDefaultAsync(p => p.RecipeId == favoriteModel.RecipeId && p.Type == 1);
                    }
                    else if (favoriteModel.MediaId != 0)
                    {
                        post = await _context.Posts.FirstOrDefaultAsync(p => p.MediaId == favoriteModel.MediaId && p.Type == 2);
                    }

                    if (post == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "Post not found for the given ID.";
                        return result;
                    }

                    var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == favoriteModel.UserId && f.PostId == post.Id);

                    if (favorite != null)
                    {
                        _context.Favorites.Remove(favorite);
                        result.Message = "Removed from favorites";
                    }
                    else
                    {
                        Favorite newFavorite = new Favorite
                        {
                            UserId = favoriteModel.UserId,
                            PostId = post.Id
                        };
                        _context.Favorites.Add(newFavorite);
                        result.Result = newFavorite;
                        result.Message = "Added to favorites";
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    result.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.IsSuccess = false;
                    result.Message = ex.Message;
                }
            }
            return result;
        }





        private async Task<int?> GetPostIdFromMediaOrRecipe(int? mediaId, int? recipeId)
        {
            if (mediaId.HasValue)
            {
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.MediaId == mediaId && p.Type == 2);
                return post?.Id;
            }
            else if (recipeId.HasValue)
            {
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.RecipeId == recipeId && p.Type == 1);
                return post?.Id;
            }

            return null; // No valid ID provided
        }


        public async Task<ResponseResult<List<UserPostsModel>>> GetFavorites(int userId, string favoriteType = null)
        {
            string recipeMediaPath = "uploads/recipes/";
            string generalMediaPath = "uploads/media/";

            var result = new ResponseResult<List<UserPostsModel>>();
            try
            {
                var favoritesQuery = _context.Favorites
                    .Where(f => f.UserId == userId);

                if (!string.IsNullOrEmpty(favoriteType))
                {
                    if (favoriteType.Equals("media", StringComparison.OrdinalIgnoreCase))
                    {
                        favoritesQuery = favoritesQuery.Where(f => f.Post.Type == 2);
                    }
                    else if (favoriteType.Equals("recipe", StringComparison.OrdinalIgnoreCase))
                    {
                        favoritesQuery = favoritesQuery.Where(f => f.Post.Type == 1);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid favorite type specified.");
                    }
                }

                var favorites = await favoritesQuery
                    .Select(f => new UserPostsModel
                    {
                        Id = f.Post.Id,
                        PostType = f.Post.Type,
                        Media = f.Post.Type == 2 ? new MediaDto
                        {
                            Id = f.Post.MediaId,
                            Title = f.Post.Media.Title,
                            Description = f.Post.Media.Description,
                            MediaData = f.Post.Media.MediaData != null ? $"{baseUrl}{generalMediaPath}{f.Post.Media.MediaData}" : null,
                            Likes = f.Post.Media.Likes,
                            CreatedAt = f.Post.Media.CreatedAt,
                            RecipeReference = f.Post.Media.RecipeId.HasValue ? new RecipeReferenceDto
                            {
                                Id = f.Post.Media.RecipeId,
                                Name = f.Post.Media.Recipe.Name
                            } : null
                        } : null,
                        Recipe = f.Post.Type == 1 ? new RecipeDto
                        {
                            Id = f.Post.RecipeId,
                            Name = f.Post.Recipe.Name,
                            Description = f.Post.Recipe.Description,
                            PreparationTime = f.Post.Recipe.PreperationTime,
                            Media = f.Post.Recipe.Media != null ? $"{baseUrl}{recipeMediaPath}{f.Post.Recipe.Media}" : null,
                            CreatedAt = f.Post.Recipe.CreatedAt,
                            AverageRating = f.Post.Recipe.Reviews.Any() ? f.Post.Recipe.Reviews.Average(r => r.Rating) : 0
                        } : null
                    })
                    .ToListAsync();

                result.IsSuccess = true;
                result.Result = favorites;
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
