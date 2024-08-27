using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.TargetUserId});
        
        builder
            .Entity<UserLike>()
            .HasOne(u => u.SourceUser)
            .WithMany(l => l.LikedUser)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .Entity<UserLike>()
            .HasOne(u => u.TargetUser)
            .WithMany(l => l.LikedByUsers)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .Entity<Message>()
            .HasOne(u => u.Recipient)
            .WithMany(l => l.MessagesRecieved)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .Entity<Message>()
            .HasOne(u => u.Sender)
            .WithMany(l => l.MessageSent)
            .OnDelete(DeleteBehavior.Restrict);
    }
}