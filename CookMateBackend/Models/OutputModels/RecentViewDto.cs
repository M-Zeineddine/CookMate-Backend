namespace CookMateBackend.Models.OutputModels
{
    public class RecentViewDto
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public DateTime ViewedAt { get; set; }
        public string? PreparationTime { get; set; }
        public string? Media { get; set; }
        public decimal AverageRating { get; set; } // Property to hold the average rating
    }

}
