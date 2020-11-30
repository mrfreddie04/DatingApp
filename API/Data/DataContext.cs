using Microsoft.EntityFrameworkCore;
using API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

    }

  }
}