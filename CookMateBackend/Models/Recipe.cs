using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Recipe
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? PreperationTime { get; set; }

    public string? Media { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Media> MediaNavigation { get; set; } = new List<Media>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();

    public virtual ICollection<RecipeDislike> RecipeDislikes { get; set; } = new List<RecipeDislike>();

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<RecipeLike> RecipeLikes { get; set; } = new List<RecipeLike>();

    public virtual ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();

    public virtual ICollection<RecipeView> RecipeViews { get; set; } = new List<RecipeView>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
