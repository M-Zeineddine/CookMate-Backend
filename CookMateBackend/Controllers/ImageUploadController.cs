using CookMateBackend.Models;
using CookMateBackend.Models.InputModels;
using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CookMateBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageProcessingController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string ApiKey = "kknAL4hyS96ioSkBipP8"; // Your actual API key
        private const string ModelEndpoint = "vegetables_v3/5"; // Your actual model endpoint
        public readonly CookMateContext _context;


        public ImageProcessingController(IHttpClientFactory clientFactory, CookMateContext cookMateContext)
        {
            _clientFactory = clientFactory;
            _context = cookMateContext;

        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new ResponseResult<string> { IsSuccess = false, Message = "An image file must be uploaded." });
            }

            string uploadURL = $"https://detect.roboflow.com/{ModelEndpoint}?api_key={ApiKey}";

            try
            {
                using (var httpClient = _clientFactory.CreateClient())
                using (var multipartFormDataContent = new MultipartFormDataContent())
                using (var fileStream = imageFile.OpenReadStream())
                using (var content = new StreamContent(fileStream))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                    multipartFormDataContent.Add(content, "file", imageFile.FileName);

                    var response = await httpClient.PostAsync(uploadURL, multipartFormDataContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, new ResponseResult<string> { IsSuccess = false, Message = await response.Content.ReadAsStringAsync() });
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var predictions = JsonConvert.DeserializeObject<List<DetectionModel>>(Convert.ToString(apiResponse.predictions));

                    // Match ingredients to those in the database
                    var matchedIngredients = await MatchIngredientsToDatabase(predictions);

                    // Return matched ingredients
                    return Ok(new ResponseResult<List<Ingredient>> { IsSuccess = true, Message = "Detected successfully", Result = matchedIngredients });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseResult<string> { IsSuccess = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        private async Task<List<Ingredient>> MatchIngredientsToDatabase(List<DetectionModel> detections)
        {
            // Filter detections by confidence level > 0.4 and normalize detected names
            var detectedNames = detections.Where(d => d.Confidence > 0.4) // Filter by confidence level
                                          .Select(d => d.Class.Replace("_", " ").ToLower()) // Normalize names
                                          .ToList();

            // Query your database for matching ingredients, assuming your ingredient names are stored in a 'normalized' format
            var matchedIngredients = await _context.Ingredients
                                                    .Where(ingredient => detectedNames.Contains(ingredient.Name.ToLower()))
                                                    .ToListAsync();

            return matchedIngredients;
        }


        [HttpGet("searchIngredients")]
        public async Task<IActionResult> SearchIngredients(string searchTerm)
        {
            var ingredients = await _context.Ingredients
                                            .Where(ing => EF.Functions.Like(ing.Name, $"%{searchTerm}%"))
                                            .ToListAsync();
            return Ok(new ResponseResult<List<Ingredient>> { IsSuccess = true, Message = "Search successful", Result = ingredients });
        }

        [HttpGet("searchRecipes")]
        public async Task<IActionResult> SearchRecipes( List<int> ingredientIds)
        {
            var recipes = await _context.Recipes
                                         .Where(r => r.RecipeIngredients
                                                      .Any(ri => ingredientIds.Contains(ri.IngredientListId)))
                                         .ToListAsync();
            return Ok(new ResponseResult<List<Recipe>> { IsSuccess = true, Message = "Search successful", Result = recipes });
        }



    }
}
