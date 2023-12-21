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



        /*public async Task<ResponseResult<List<UserMediasModel>>> GetUserMedia(int userId)
        {
            try
            {
                var media = await _CookMateContext.UserMediasModel
                    .FromSqlRaw("EXEC GetUserMedia @UserId", new SqlParameter("@UserId", userId))
                    .ToListAsync();

                var result = new ResponseResult<List<UserMediasModel>>();

                if (media != null && media.Any())
                {
                    result.IsSuccess = true;
                    result.Message = "User media retrieved successfully.";
                    result.Result = media;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "User has no media.";
                    result.Result = new List<UserMediasModel>();
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ResponseResult<List<UserMediasModel>>
                {
                    IsSuccess = false,
                    Message = $"An error occurred: {ex.Message}",
                    Result = null
                };
            }
        }*/

    }
}
