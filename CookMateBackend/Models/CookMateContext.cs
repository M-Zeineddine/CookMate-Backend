using System;
using System.Collections.Generic;
using CookMateBackend.Models.OutputModels;
using Microsoft.EntityFrameworkCore;

namespace CookMateBackend.Models;

public partial class CookMateContext : DbContext
{
    public CookMateContext()
    {
    }

    public CookMateContext(DbContextOptions<CookMateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientSubstitute> IngredientSubstitutes { get; set; }

    public virtual DbSet<InteractionHistory> InteractionHistories { get; set; }

    public virtual DbSet<Media> Media { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Procedure> Procedures { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeDislike> RecipeDislikes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<RecipeLike> RecipeLikes { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    public virtual DbSet<RecipeView> RecipeViews { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SearchHistory> SearchHistories { get; set; }

    public virtual DbSet<TagCategory> TagCategories { get; set; }

    public virtual DbSet<TagsList> TagsLists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPreferencesTag> UserPreferencesTags { get; set; }


    //OutputModels
    public virtual DbSet<UserDetailsModel> UserDetailsModel { get; set; }
    public virtual DbSet<UserPostsModel> UserPostsModel { get; set; }
    /*public virtual DbSet<UserMediasModel> UserMediasModel { get; set; }*/
    public virtual DbSet<MediaDto> MediaModels { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-OFBT92P\\SQL2022;Database=CookMate;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("favorites");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_post_id");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorites_user_id");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_1");

            entity.ToTable("followers");

            entity.HasIndex(e => e.FollowerId, "follower_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.FollowerNavigation).WithMany(p => p.FollowerFollowerNavigations)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("follower_id");

            entity.HasOne(d => d.User).WithMany(p => p.FollowerUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_id");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ingredients_id");

            entity.ToTable("ingredients");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<IngredientSubstitute>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ingredient_substitutes");

            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.SubstituteId).HasColumnName("substitute_id");

            entity.HasOne(d => d.Ingredient).WithMany()
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("FK__ingredien__ingre__72910220");

            entity.HasOne(d => d.Substitute).WithMany()
                .HasForeignKey(d => d.SubstituteId)
                .HasConstraintName("FK__ingredien__subst__73852659");
        });

        modelBuilder.Entity<InteractionHistory>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK__interact__605F8FE61CB80C0F");

            entity.ToTable("interaction_history");

            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.InteractionHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__interacti__user___3C34F16F");
        });

        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_media_id");

            entity.ToTable("media");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.MediaData).HasColumnName("media_data");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Recipe).WithMany(p => p.MediaNavigation)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_media_recipe_id");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_post_id");

            entity.ToTable("posts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Media).WithMany(p => p.Posts)
                .HasForeignKey(d => d.MediaId)
                .HasConstraintName("FK_post_media_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Posts)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_post_recipe_id");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_post_user_id");
        });

        modelBuilder.Entity<Procedure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_procedures_id");

            entity.ToTable("procedures");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Media)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("media");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.Step).HasColumnName("step");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Procedures)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_procedures_recipe_id");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_recipe_id");

            entity.ToTable("recipes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Media)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("media");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PreperationTime).HasColumnName("preperation_time");
        });

        modelBuilder.Entity<RecipeDislike>(entity =>
        {
            entity.HasKey(e => e.DislikeId).HasName("PK__recipe_d__63D9DC376F031713");

            entity.ToTable("recipe_dislikes");

            entity.Property(e => e.DislikeId).HasColumnName("dislike_id");
            entity.Property(e => e.DislikedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("disliked_at");
            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");

            entity.HasOne(d => d.Interaction).WithMany(p => p.RecipeDislikes)
                .HasForeignKey(d => d.InteractionId)
                .HasConstraintName("FK__recipe_di__inter__498EEC8D");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeDislikes)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__recipe_di__recip__4A8310C6");
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_recipe_ingredients_id");

            entity.ToTable("recipe_ingredients");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IngredientListId).HasColumnName("ingredient_list_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.Weight)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("weight");

            entity.HasOne(d => d.IngredientList).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.IngredientListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_ingredients_ingredient_list_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeIngredients)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_ingredients_recipe_id");
        });

        modelBuilder.Entity<RecipeLike>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__recipe_l__992C79306A831DE4");

            entity.ToTable("recipe_likes");

            entity.Property(e => e.LikeId).HasColumnName("like_id");
            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.LikedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("liked_at");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");

            entity.HasOne(d => d.Interaction).WithMany(p => p.RecipeLikes)
                .HasForeignKey(d => d.InteractionId)
                .HasConstraintName("FK__recipe_li__inter__44CA3770");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeLikes)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__recipe_li__recip__45BE5BA9");
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_recipe_tags_id");

            entity.ToTable("recipe_tags");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.TagListId).HasColumnName("tag_list_id");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_tags_recipe_id");

            entity.HasOne(d => d.TagList).WithMany(p => p.RecipeTags)
                .HasForeignKey(d => d.TagListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_recipe_tags_tag_list_id");
        });

        modelBuilder.Entity<RecipeView>(entity =>
        {
            entity.HasKey(e => e.ViewId).HasName("PK__recipe_v__B5A34EE2D647D1E6");

            entity.ToTable("recipe_views");

            entity.Property(e => e.ViewId).HasColumnName("view_id");
            entity.Property(e => e.InteractionId).HasColumnName("interaction_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.ViewedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("viewed_at");

            entity.HasOne(d => d.Interaction).WithMany(p => p.RecipeViews)
                .HasForeignKey(d => d.InteractionId)
                .HasConstraintName("FK__recipe_vi__inter__40058253");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeViews)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK__recipe_vi__recip__40F9A68C");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_review_id");

            entity.ToTable("review");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("comment");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("rating");
            entity.Property(e => e.RecipesId).HasColumnName("recipes_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Recipes).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RecipesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_review_recipes_id");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_review_user_id");
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => e.SearchId).HasName("PK__search_h__B302268D21D8F7E3");

            entity.ToTable("search_history");

            entity.Property(e => e.SearchId).HasColumnName("search_id");
            entity.Property(e => e.SearchTerm).HasColumnName("search_term");
            entity.Property(e => e.SearchedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("searched_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.SearchHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__search_hi__user___4E53A1AA");
        });

        modelBuilder.Entity<TagCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tag_category_id");

            entity.ToTable("tag_category");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsDuplicatable).HasColumnName("is_duplicatable");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TagsList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tags_list_id");

            entity.ToTable("tags_list");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.TagCategoryId).HasColumnName("tag_category_id");

            entity.HasOne(d => d.TagCategory).WithMany(p => p.TagsLists)
                .HasForeignKey(d => d.TagCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tags_list_tag_category_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_user_id");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bio)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("bio");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.ProfilePic)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("profile_pic");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserPreferencesTag>(entity =>
        {
            entity.HasKey(e => e.PreferenceTagId).HasName("PK__user_pre__ABA8BEFDDC458365");

            entity.ToTable("user_preferences_tags");

            entity.Property(e => e.PreferenceTagId).HasColumnName("preference_tag_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Tag).WithMany(p => p.UserPreferencesTags)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("FK__user_pref__tag_i__531856C7");

            entity.HasOne(d => d.User).WithMany(p => p.UserPreferencesTags)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__user_pref__user___5224328E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
