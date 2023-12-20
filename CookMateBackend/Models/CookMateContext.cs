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

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Procedure> Procedures { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<TagCategory> TagCategories { get; set; }

    public virtual DbSet<TagsList> TagsLists { get; set; }

    public virtual DbSet<User> Users { get; set; }



    //OutputModels
    public virtual DbSet<UserDetailsModel> UserDetailsModel { get; set; }
    public virtual DbSet<UserPostsModel> UserPostsModel { get; set; }
    public virtual DbSet<UserMediasModel> UserMediasModel { get; set; }


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

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_media_id");

            entity.ToTable("media");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descirption)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descirption");
            entity.Property(e => e.MediaData)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("media_data");
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
                .HasMaxLength(50)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
