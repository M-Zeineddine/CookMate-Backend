using CookMateBackend.Data.Interfaces;
using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;

/*using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;*/


namespace CookMateBackend.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        public readonly CookMateContext _CookMateContext;

        public AuthService(IConfiguration configuration, CookMateContext cookMateContext)
        {
            _configuration = configuration;
            _CookMateContext = cookMateContext;
        }

        /*private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        // You can add more claims if needed
    };

            var token = new JwtSecurityToken(
                issuer: builder.Configuration["JwtSettings:Issuer"],
                audience: builder.Configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token validity period (1 hour for this example)
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }*/

        public User? Register(RegisterModel model)
        {
            // Check if the username or email already exists
            if (_CookMateContext.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
            {
                return null;
            }

            // Create a new user
            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = BC.HashPassword(model.Password),
                Role = 2,
                ProfilePic = model.ProfilePic,
                Bio = null
            };

            // Add to the database and save
            _CookMateContext.Users.Add(newUser);
            _CookMateContext.SaveChanges();

            return newUser;  // or just return a success message or token
        }


        public string GenerateJwtToken(User user)
        {

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenValidity = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:TokenValidity"]));
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);


            var claims = new List<Claim>
            {
                new Claim("TokenGUID", Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Username", user.Username),
                /*new Claim("Role", user.Role.ToString())*/
            };

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = tokenValidity,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha512),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return token;
        }

        public bool IsAuthenticated(string username, string password)
        {
            var user = GetUserByUsername(username);
            return DoesUsernameExists(username) && BC.Verify(password, user.Password);
        }


        public bool DoesUsernameExists(string username)
        {
            var user = _CookMateContext.Users.FirstOrDefault(x => x.Username == username);
            return user != null;
        }
        public User GetUserByUsername(string username)
        {
            return _CookMateContext.Users.FirstOrDefault(c => c.Username == username);
        }
    }
}
