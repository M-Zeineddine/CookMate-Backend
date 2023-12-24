using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class SearchHistory
{
    public int SearchId { get; set; }

    public int? UserId { get; set; }

    public string? SearchTerm { get; set; }

    public DateTime? SearchedAt { get; set; }

    public virtual User? User { get; set; }
}
