namespace CookMateBackend.Models.OutputModels
{
    public class RecipeSearchResultModel
    {
        public int? RecipeId { get; set; }
        public string? RecipeName { get; set; }
        public string? OwnerUsername { get; set; }
    }
}
