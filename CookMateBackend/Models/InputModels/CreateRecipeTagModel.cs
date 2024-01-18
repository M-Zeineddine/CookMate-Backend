namespace CookMateBackend.Models.InputModels
{

    public class CreateRecipeTagModel
    {
        public int TagListId { get; set; }
        public int RecipeId { get; set; } // Assuming this is needed to associate the tag with a recipe
    }
}
