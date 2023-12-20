using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Recipe
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int PreperationTime { get; set; }

    public string Media { get; set; } = null!;

    public virtual ICollection<Medium> MediaNavigation { get; set; } = new List<Medium>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
