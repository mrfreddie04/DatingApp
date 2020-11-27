using Microsoft.EntityFrameworkCore;
using API.Entities;

namespace API.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    //public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

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
    }

  }
}