using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Data.Repositories
{
    public class PostRepository: IPostRepository
    {
        public readonly CookMateContext _CookMateContext;

        public PostRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }

        public async Task<ResponseResult<List<UserPostsModel>>> GetPostsByUserId(int userId, int postType)
        {
            var result = new ResponseResult<List<UserPostsModel>>();

            try
            {
                var query = _CookMateContext.Posts.AsQueryable();

                if (postType == 1)
                {
                    query = query.Include(p => p.Recipe);
                }
                else if (postType == 2)
                {
                    query = query.Include(p => p.Media);
                }

                var postsWithDetails = await query
                    .Where(p => p.UserId == userId && p.Type == postType)
                    .ToListAsync();

                var userPosts = postsWithDetails
                    .Where(p => (postType == 1 && p.Recipe != null) || (postType == 2 && p.Media != null)) // Additional check to avoid NullReferenceException
                    .Select(p => new UserPostsModel
                    {
                        Id = p.Id,
                        PostType = p.Type,
                        Recipe = postType == 1 ? new RecipeDto
                        {
                            Id = p.Recipe.Id,
                            Name = p.Recipe.Name,
                            Description = p.Recipe.Description,
                            PreparationTime = p.Recipe.PreperationTime,
                            Media = p.Recipe.Media
                            // Map other recipe properties
                        } : null,
                        Media = postType == 2 ? new MediaDto
                        {
                            Id = p.Media.Id,
                            Title = p.Media.Title,
                            Description = p.Media.Description,
                            MediaType = p.Media.MediaType      
                            // Map other media properties
                        } : null
                    }).ToList();

                result.IsSuccess = true;
                result.Result = userPosts;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message; // You might want to log this instead of sending it to the client.
            }

            return result;
        }

    }
}
