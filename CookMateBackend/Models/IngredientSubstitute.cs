using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class IngredientSubstitute
{
    public int? IngredientId { get; set; }

    public int? SubstituteId { get; set; }

    public virtual Ingredient? Ingredient { get; set; }

    public virtual Ingredient? Substitute { get; set; }
}
