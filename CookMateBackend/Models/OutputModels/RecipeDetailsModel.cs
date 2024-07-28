namespace CookMateBackend.Models.OutputModels
{
    public class RecipeDetailsModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PreparationTime { get; set; }
        public string? Media { get; set; }
        public bool IsCreatedByUser { get; set; } // New property
        public int ViewCount { get; set; }
        public bool IsFollowing { get; set; } // New property indicating if the logged-in user is following the recipe owner
        public UserModel? User { get; set; }
        public List<Procedure>? Procedures { get; set; }
        public List<IngredientDto>? Ingredients { get; set; }
        public List<TagDto>? Tags { get; set; }

    }

    public class IngredientDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; } // The name of the ingredient from the Ingredient table.
        public decimal? Weight { get; set; } // The weight of the ingredient for this particular recipe from the RecipeIngredient table.
        public string? MediaUrl { get; set; } // The URL to the media of the ingredient.
    }

    public class TagDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        // Add additional properties as needed
    }

}
