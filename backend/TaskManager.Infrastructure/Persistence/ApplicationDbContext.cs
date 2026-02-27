using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUsers(modelBuilder);
            ConfigureTasks(modelBuilder);
        }

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<User>();

            entity.ToTable("users");

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(u => u.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(u => u.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();
        }

        private static void ConfigureTasks(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<TaskItem>();

            entity.ToTable("tasks");

            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(t => t.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(t => t.Title);

            entity.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            entity.Property(t => t.DueDate)
                .HasColumnName("due_date");

            entity.HasIndex(t => t.DueDate);

            entity.Property(t => t.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(t => t.Status);

            entity.Property(t => t.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(1000);

            entity.Property(t => t.CreatedOn)
                .HasColumnName("created_on")
                .IsRequired();

            entity.Property(t => t.UpdatedOn)
                .HasColumnName("updated_on")
                .IsRequired();

            entity.Property(t => t.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();

            entity.Property(t => t.UpdatedByUserId)
                .HasColumnName("updated_by_user_id")
                .IsRequired();

            entity.HasIndex(t => t.CreatedByUserId);

            entity.HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.UpdatedByUser)
                .WithMany(u => u.UpdatedTasks)
                .HasForeignKey(t => t.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
