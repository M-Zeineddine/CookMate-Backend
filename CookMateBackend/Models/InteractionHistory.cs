using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class InteractionHistory
{
    public int InteractionId { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RecipeDislike> RecipeDislikes { get; set; } = new List<RecipeDislike>();

    public virtual ICollection<RecipeLike> RecipeLikes { get; set; } = new List<RecipeLike>();

    public virtual ICollection<RecipeView> RecipeViews { get; set; } = new List<RecipeView>();

    public virtual User? User { get; set; }
}
