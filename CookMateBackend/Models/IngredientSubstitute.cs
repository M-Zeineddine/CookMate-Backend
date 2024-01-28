using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public class IngredientSubstitute
{
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }
    public int SubstituteId { get; set; }
    public Ingredient Substitute { get; set; }
}

