namespace CookMateBackend.Models.InputModels
{
    public class FavoriteModel
    {
        public int UserId { get; set; }
        public int? MediaId { get; set; }
        public int? RecipeId { get; set; }
    }

}
