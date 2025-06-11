using Microsoft.EntityFrameworkCore;
using StudyMess_Server.Models;

namespace StudyMess_Server.Data
{
    public class StudyMessDbContext : DbContext
    {
        public StudyMessDbContext(DbContextOptions<StudyMessDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<FileMessage> FileMessages => Set<FileMessage>();
        public DbSet<Group> Groups => Set<Group>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne<Group>()
                .WithMany()
                .HasForeignKey(u => u.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMember>()
                .HasIndex(cm => new { cm.ChatId, cm.UserId })
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(50);

            modelBuilder.Entity<ChatMember>()
                .Property(cm => cm.Role)
                .HasMaxLength(20);

            modelBuilder.Entity<FileMessage>()
                .Ignore(fm => fm.File);
        }
    }
}