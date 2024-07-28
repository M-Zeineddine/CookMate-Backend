using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // For IFormFile

namespace CookMateBackend.Models.InputModels
{
    public class EditPostModel : IValidatableObject
    {
        [Required]
        public int PostId { get; set; } // Identifies the post to be edited

        [Required]
        public byte Type { get; set; } // 1 for Recipe, 2 for Media

        [Required]
        public int UserId { get; set; } // To ensure that only the owner can edit the post

        // Optional properties for editing; not all properties need to be provided for an edit
        public string? RecipeName { get; set; }
        public string? RecipeDescription { get; set; }
        public int? PreparationTime { get; set; }
        public IFormFile? RecipeMedia { get; set; } // New image for the recipe, if provided

        public string? MediaTitle { get; set; }
        public string? MediaDescription { get; set; }
        public IFormFile? MediaData { get; set; } // New image for the media post, if provided
        public int? RecipeId { get; set; } // For media posts related to a recipe

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == 1)
            {
                if (string.IsNullOrEmpty(RecipeName) && RecipeMedia == null && !PreparationTime.HasValue && string.IsNullOrEmpty(RecipeDescription))
                    yield return new ValidationResult("At least one field (RecipeName, RecipeMedia, PreparationTime, RecipeDescription) must be provided for recipe posts.", new[] { nameof(RecipeName), nameof(RecipeMedia), nameof(PreparationTime), nameof(RecipeDescription) });
            }
            else if (Type == 2)
            {
                if (string.IsNullOrEmpty(MediaTitle) && MediaData == null)
                    yield return new ValidationResult("At least one field (MediaTitle, MediaData) must be provided for media posts.", new[] { nameof(MediaTitle), nameof(MediaData) });
            }
        }
    }
}
