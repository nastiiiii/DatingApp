using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, 
    AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().HasMany(ur => ur.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId)
            .IsRequired();
        builder.Entity<AppRole>().HasMany(ur => ur.UserRoles).WithOne(u => u.Role).HasForeignKey(ur => ur.RoleId)
            .IsRequired();
        
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
        
        builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
    }
}