using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
