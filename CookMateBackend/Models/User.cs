using System;
using System.Collections.Generic;

namespace CookMateBackend.Models;

public partial class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public byte Role { get; set; }

    public string? ProfilePic { get; set; }

    public string? Bio { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Follower> FollowerFollowerNavigations { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerUsers { get; set; } = new List<Follower>();

    public virtual ICollection<InteractionHistory> InteractionHistories { get; set; } = new List<InteractionHistory>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    public virtual ICollection<UserPreferencesTag> UserPreferencesTags { get; set; } = new List<UserPreferencesTag>();
    public virtual ICollection<MediaComment> MediaComments { get; set; }

}
