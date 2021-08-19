using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder builder) 
    {
      base.OnModelCreating(builder);

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


    }
  }
}