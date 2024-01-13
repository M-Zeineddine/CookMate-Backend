using CookMateBackend.Data.Interfaces;
using CookMateBackend.Data.Repositories;
using CookMateBackend.Models.InputModels;
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
        public ActionResult<string> Login(LoginModel userModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(userModel.Username))
                    {
                        return BadRequest("Username is required");
                    }

                    if (string.IsNullOrEmpty(userModel.Password))
                    {
                        return BadRequest("Password is required");
                    }


                    if (_authService.IsAuthenticated(userModel.Username, userModel.Password))
                    {
                        var user = _authService.GetUserByUsername(userModel.Username);
                        var token = _authService.GenerateJwtToken(user);


                        //return Ok(user.Username + " is now connected");  //returns rayba123 is now conected
                        return Ok(token);
                    }

                    return BadRequest("Username or password are not correct!");
                }

                return BadRequest(ModelState);

            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

    }
}
