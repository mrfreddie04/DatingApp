using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using API.Entities;

namespace API.Data
{
  public class DataContext : IdentityDbContext<
    AppUser,AppRole, int, 
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, 
    IdentityRoleClaim<int>, IdentityUserToken<int>
  >
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    //public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) 
    {
      base.OnModelCreating(builder);

      // builder.Entity<Group>()
      //   .HasMany( g => g.Connections)
      //   .WithOne( c => c.)

      builder.Entity<AppUser>()
        .HasMany( au => au.UserRoles)
        .WithOne( aur => aur.User)
        .HasForeignKey( aur => aur.UserId)
        .IsRequired();

      builder.Entity<AppRole>()
        .HasMany( ar => ar.UserRoles)
        .WithOne( aur => aur.Role)
        .HasForeignKey( aur => aur.RoleId)
        .IsRequired();        

      builder.Entity<UserLike>()
        .HasKey( t => new {t.SourceUserId, t.LikedUserId});

      builder.Entity<UserLike>()
        .HasOne( ul => ul.SourceUser)
        .WithMany( au => au.LikedUsers)
        .HasForeignKey( ul => ul.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<UserLike>()  
        .HasOne( ul => ul.LikedUser)
        .WithMany( au => au.LikedByUsers)
        .HasForeignKey( ul => ul.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<Message>()  
        .HasOne( m => m.Recipient)
        .WithMany( au => au.MessagesReceived)
        .HasForeignKey( m => m.RecipientId)
        .OnDelete( DeleteBehavior.Restrict);

      builder.Entity<Message>()  
        .HasOne( m => m.Sender)
        .WithMany( au => au.MessagesSent)
        .HasForeignKey( m => m.SenderId)
        .OnDelete( DeleteBehavior.Restrict);

      builder.ApplyUtcDateTimeConverter();
    }
  }

  public static class UtcDateAnnotation
  {
    private const String IsUtcAnnotation = "IsUtc";
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
      new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
      new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

    public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
      builder.HasAnnotation(IsUtcAnnotation, isUtc);

    public static Boolean IsUtc(this IMutableProperty property) =>
      ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

    /// <summary>
    /// Make sure this is called after configuring all your entities.
    /// </summary>
    public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
    {
      foreach (var entityType in builder.Model.GetEntityTypes())
      {
        foreach (var property in entityType.GetProperties())
        {
          if (!property.IsUtc())
          {
            continue;
          }

          if (property.ClrType == typeof(DateTime))
          {
            property.SetValueConverter(UtcConverter);
          }

          if (property.ClrType == typeof(DateTime?))
          {
            property.SetValueConverter(UtcNullableConverter);
          }
        }
      }
    }
  }

}