using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
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
        public IActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newUser = _authService.Register(model);

            if (newUser == null)
                return BadRequest("Username or email is already taken.");

            return Ok(newUser);  // or just return a success message or token
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



    }
}
