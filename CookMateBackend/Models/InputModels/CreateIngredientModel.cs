namespace CookMateBackend.Models.InputModels
{
    public class CreateIngredientModel
    {
        public string Name { get; set; }
        public IFormFile? Image { get; set; }
    }
}
