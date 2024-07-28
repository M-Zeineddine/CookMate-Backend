using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private AuthService _authService;
        private readonly IUserRepository _userRepository;
        public AuthController(AuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("Register")]
        public ActionResult<ResponseResult<string>> Register(RegisterModel model)
        {
            var response = new ResponseResult<string>();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid model state";
                    return new ActionResult<ResponseResult<string>>(response);
                }

                // Handle the profile picture upload
                string profilePicFilename = null;
                if (model.ProfilePic != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads/profilepics");
                    // Ensure the uploads directory exists
                    Directory.CreateDirectory(uploadsFolder);

                    // Generate a unique filename to avoid file name conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePic.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfilePic.CopyTo(fileStream);
                    }

                    // Save the path or filename to the user's profile
                    profilePicFilename = uniqueFileName;
                }

                // Now, pass the filename to the Register method or adjust your User model accordingly
                var newUser = _authService.Register(model, profilePicFilename);

                if (newUser == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Username or email is already taken.";
                    return new ActionResult<ResponseResult<string>>(response);
                }

                // Assuming GenerateJwtToken creates a JWT token for the new user
                var token = _authService.GenerateJwtToken(newUser);

                response.IsSuccess = true;
                response.Message = "Registration successful";
                response.Result = token; // Send the JWT token as the result
                return new ActionResult<ResponseResult<string>>(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Internal server error: {ex.Message}";
                return new ActionResult<ResponseResult<string>>(response);
            }
        }



        [HttpPost("Login")]
        public ActionResult<ResponseResult<string>> Login(LoginModel userModel)
        {
            var response = new ResponseResult<string>();

            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(userModel.Username))
                    {
                        response.IsSuccess = false;
                        response.Message = "Username is required";
                        return new ActionResult<ResponseResult<string>>(response);
                    }

                    if (string.IsNullOrEmpty(userModel.Password))
                    {
                        response.IsSuccess = false;
                        response.Message = "Password is required";
                        return new ActionResult<ResponseResult<string>>(response);
                    }

                    if (_authService.IsAuthenticated(userModel.Username, userModel.Password))
                    {
                        var user = _authService.GetUserByUsername(userModel.Username);
                        var token = _authService.GenerateJwtToken(user);

                        response.IsSuccess = true;
                        response.Message = "Logged in successfuly";
                        response.Result = token;
                        return new ActionResult<ResponseResult<string>>(response);
                    }

                    response.IsSuccess = false;
                    response.Message = "Username or password are not correct!";
                    return new ActionResult<ResponseResult<string>>(response);
                }

                response.IsSuccess = false;
                response.Message = "Invalid model state";
                return new ActionResult<ResponseResult<string>>(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Internal server error: {ex.Message}";
                return new ActionResult<ResponseResult<string>>(response);
            }
        }


        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _authService.ChangePassword(model);
            if (!result)
            {
                return BadRequest("Current password is incorrect or user does not exist.");
            }

            return Ok(new { message = "Password updated successfully." });
        }



    }
}
