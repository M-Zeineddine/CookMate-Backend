namespace CookMateBackend.Models.OutputModels
{
    public class SearchResultItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int? TagCategoryId { get; set; } // Use nullable int for TagCategoryId to handle the '0' value for recipes and users
    }

}
