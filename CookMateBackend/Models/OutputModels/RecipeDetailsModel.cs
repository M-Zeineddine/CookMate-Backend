namespace CookMateBackend.Models.OutputModels
{
    public class RecipeDetailsModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PreparationTime { get; set; }
        public string? Media { get; set; }
        public UserModel? User { get; set; }
        public List<Procedure>? Procedures { get; set; }
        public List<IngredientDto>? Ingredients { get; set; }

    }

    public class IngredientDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; } // The name of the ingredient from the Ingredient table.
        public decimal? Weight { get; set; } // The weight of the ingredient for this particular recipe from the RecipeIngredient table.
    }


}
