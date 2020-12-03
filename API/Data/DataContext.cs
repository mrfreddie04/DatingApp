using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using API.Entities;

namespace API.Data
{
  public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, 
    AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserLike> Likes { get; set; }
    //public DbSet<Photo> Photos { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      //configure identity tables
      builder.Entity<AppUser>()
        .HasMany(u=>u.UserRoles)
        .WithOne(ur=>ur.User)
        .HasForeignKey(ur=>ur.UserId)
        .IsRequired();
    
      builder.Entity<AppRole>()
        .HasMany(r=>r.UserRoles)
        .WithOne(ur=>ur.Role)
        .HasForeignKey(ur=>ur.RoleId)
        .IsRequired();      

      //builder.ApplyConfiguration<AppUser>(pass configuration class);
      //or configure everything inline
      builder.Entity<UserLike>()
        .HasKey(k=>new {k.SourceUserId,k.LikedUserId});

      builder.Entity<UserLike>()  
        .HasOne(s => s.SourceUser)
        .WithMany(u => u.LikedUsers)
        .HasForeignKey(s => s.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<UserLike>()          
        .HasOne(l => l.LikedUser)
        .WithMany(u => u.LikedByUsers)
        .HasForeignKey(l => l.LikedUserId)
        .OnDelete(DeleteBehavior.Cascade);   

      builder.Entity<Message>()
        .HasOne(m => m.Sender)
        .WithMany(u=> u.MessagesSent)
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.Restrict); 

      builder.Entity<Message>()
        .HasOne(m => m.Recipient)
        .WithMany(u=> u.MessagesReceived)
        .HasForeignKey(m => m.RecipientId)
        .OnDelete(DeleteBehavior.Restrict);   

      builder.Entity<Photo>()
        .HasQueryFilter(p => p.isApproved);

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