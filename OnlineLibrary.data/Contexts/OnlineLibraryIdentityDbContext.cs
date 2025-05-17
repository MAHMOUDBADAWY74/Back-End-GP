using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Data.Contexts
{
    public class OnlineLibraryIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<PendingUserChange> PendingUserChanges { get; set; }
        public DbSet<BooksDatum> BooksData { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FavoriteBook> FavoriteBook { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Readlist> Readlists { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityMember> CommunityMembers { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostUnlike> PostUnlikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<ExchangeBookRequestx> exchangeBooksRequests { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Visit> Visits { get; set; } 

        public OnlineLibraryIdentityDbContext(DbContextOptions<OnlineLibraryIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<ExchangeBookRequestx>()
                .HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExchangeBookRequestx>()
                .HasOne(e => e.Receiver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostUnlike>()
                .HasOne(pu => pu.Post)
                .WithMany()
                .HasForeignKey(pu => pu.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Receiver)
                .WithMany()
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}