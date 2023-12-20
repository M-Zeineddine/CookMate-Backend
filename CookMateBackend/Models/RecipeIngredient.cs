using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class RecipeIngredient
{
    public int Id { get; set; }

    public decimal Weight { get; set; }

    public int RecipeId { get; set; }

    public int IngredientListId { get; set; }

    public virtual Ingredient IngredientList { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;
}
