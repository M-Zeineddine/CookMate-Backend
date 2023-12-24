using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class RecipeDislike
{
    public int DislikeId { get; set; }

    public int? InteractionId { get; set; }

    public int? RecipeId { get; set; }

    public DateTime? DislikedAt { get; set; }

    public virtual InteractionHistory? Interaction { get; set; }

    public virtual Recipe? Recipe { get; set; }
}
