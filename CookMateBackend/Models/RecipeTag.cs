using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CookMateBackend.Models;

public partial class RecipeTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int TagListId { get; set; }

    public virtual Recipe? Recipe { get; set; }

    public virtual TagsList? TagList { get; set; }
}
