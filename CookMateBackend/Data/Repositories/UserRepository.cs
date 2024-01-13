using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Data.Repositories
{
    public class UserRepository: IUserRepository
    {

        public readonly CookMateContext _CookMateContext;

        public UserRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }

        /////
        ///


        public async Task<ResponseResult<List<User>>> GetAllUsers()
        {
            var list = await _CookMateContext.Users.FromSqlRaw($"exec [dbo].[GetUserDetailsById]").ToListAsync();

            var result = new ResponseResult<List<User>>();
            result.IsSuccess = true;
            result.Message = "testestest";
            result.Result = list;
            return result;
        }

        /*public async Task<ResponseResult<UserDetailsModel>> GetUserDetailsById(int userId)
        {
            try
            {
                var userDetails = await _CookMateContext.UserDetailsModel
                    .FromSqlRaw("EXEC GetUserDetailsById @UserId", new SqlParameter("@UserId", userId))
                    .FirstOrDefaultAsync();

                var result = new ResponseResult<UserDetailsModel>();


                return result;
            }
            catch (Exception ex)
            {
                return new ResponseResult<UserDetailsModel>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                };
            }
        }*/



        public async Task<ResponseResult<UserDetailsModel>> GetUserById(int userid)
        {
            var useridParameter = new SqlParameter("Userid", userid);
            var res = await _CookMateContext.UserDetailsModel.FromSqlRaw($"exec [dbo].[GetUserDetailsById] @Userid", useridParameter).ToListAsync();


            var result = new ResponseResult<UserDetailsModel>();
            result.IsSuccess = true;
            result.Message = "Should work";
            result.Result = res.FirstOrDefault();

            return result;
        }


        public async Task<List<SearchHistory>> GetSearchHistoryByUserIdAsync(int userId)
        {
            return await _CookMateContext.SearchHistories
                .Where(sh => sh.UserId == userId)
                .OrderByDescending(sh => sh.SearchedAt)
                .ToListAsync();
        }


        /*public async Task<bool> SaveSearchTermAsync(string userId, string searchTerm)
        {
            try
            {
                // Create a new search history object
                var searchHistory = new SearchHistory
                {
                    UserId = userId,
                    SearchTerm = searchTerm,
                    SearchedAt = DateTime.UtcNow // Assuming you want to save the search time in UTC
                };

                // Add the new search history to the context
                await _CookMateContext.SearchHistories.AddAsync(searchHistory);

                // Save the changes back to the database
                await _CookMateContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception and handle it appropriately
                // For example:
                // _logger.LogError("An error occurred when saving the search term: {Exception}", ex);
                return false;
            }
        }*/


        /*public async Task<ResponseResult<List<User>>> SearchUsersAsync(string searchString)
        {
            var users = await _CookMateContext.Users
                .Where(u => u.Username.Contains(searchString) || u.Email.Contains(searchString))
                .ToListAsync();

            return new ResponseResult<List<User>>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully.",
                Result = users
            };
        }

        public async Task<ResponseResult<List<Recipe>>> SearchRecipesAsync(string searchString, int? preparationTime)
        {
            var query = _CookMateContext.Recipes.AsQueryable();
            if (preparationTime.HasValue)
            {
                query = query.Where(r => Int32.Parse(r.PreperationTime) <= preparationTime.Value);
            }
            query = query.Where(r => r.Name.Contains(searchString) || r.Description.Contains(searchString));

            var recipes = await query.ToListAsync();
            return new ResponseResult<List<Recipe>>
            {
                IsSuccess = true,
                Message = "Recipes retrieved successfully.",
                Result = recipes
            };
        }

        public async Task<bool> SaveSearchTermAsync(string userId, string searchTerm)
        {
            try
            {
                var searchHistory = new SearchHistory
                {
                    UserId = userId,
                    SearchTerm = searchTerm,
                    SearchedAt = DateTime.UtcNow
                };
                _CookMateContext.SearchHistories.Add(searchHistory);
                await _CookMateContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Handle exception appropriately
                return false;
            }
        }*/

    }
}
