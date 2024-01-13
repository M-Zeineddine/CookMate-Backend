using System.ComponentModel.DataAnnotations;

namespace CookMateBackend.Models.InputModels
{
    public class CreatePostModel: IValidatableObject
    {
        [Required]
        public byte Type { get; set; } // 1 for Recipe, 2 for Media
        [Required]
        public int UserId { get; set; }

        // Recipe data
        public required string RecipeName { get; set; }
        public string? RecipeDescription { get; set; }
        public int? PreparationTime { get; set; }
        public IFormFile? RecipeMedia { get; set; } // Image for recipe

        // Media data
        public string? MediaTitle { get; set; }
        public string? MediaDescription { get; set; }
        public IFormFile? MediaData { get; set; } // Image for media post
        public int? RecipeId { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == 1)
            {
                if (string.IsNullOrEmpty(RecipeName))
                    yield return new ValidationResult("RecipeName is required for recipe posts.", new[] { nameof(RecipeName) });

                if (RecipeMedia == null)
                    yield return new ValidationResult("RecipeMedia is required for recipe posts.", new[] { nameof(RecipeMedia) });

                if (!PreparationTime.HasValue)
                    yield return new ValidationResult("PreparationTime is required for recipe posts.", new[] { nameof(PreparationTime) });

                if (string.IsNullOrEmpty(RecipeDescription))
                    yield return new ValidationResult("RecipeDescription is required for recipe posts.", new[] { nameof(RecipeDescription) });
            }
            else if (Type == 2)
            {
                if (string.IsNullOrEmpty(MediaTitle))
                    yield return new ValidationResult("MediaTitle is required for media posts.", new[] { nameof(MediaTitle) });

                if (MediaData == null)
                    yield return new ValidationResult("MediaData is required for media posts.", new[] { nameof(MediaData) });
            }
        }
    }

    public class IngredientModel
    {
        public decimal Weight { get; set; }
        public int IngredientListId { get; set; }
        public int RecipeId { get; set; }  // Add this
    }

    public class ProcedureModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? Media { get; set; }  // Assuming this is how you handle media files
        public int Time { get; set; }
        public byte Step { get; set; }
        public int RecipeId { get; set; }  // Add this
    }


}
