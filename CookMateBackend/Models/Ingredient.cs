using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Ingredient
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public string? Media { get; set; }

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();


}
