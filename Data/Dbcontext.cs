using Microsoft.EntityFrameworkCore;
using ATS.API.Models;

namespace ATS.API.Data
{
    public class AtsDbContext : DbContext
    {
        public AtsDbContext(DbContextOptions<AtsDbContext> options) : base(options)
        {
        }

        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // JobPosting configuration
            modelBuilder.Entity<JobPosting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Applicant configuration
            modelBuilder.Entity<Applicant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.EducationLevel).HasConversion<string>();
            });

            // Application configuration
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                
                entity.HasOne(e => e.JobPosting)
                    .WithMany(j => j.Applications)
                    .HasForeignKey(e => e.JobPostingId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Applicant)
                    .WithMany(a => a.Applications)
                    .HasForeignKey(e => e.ApplicantId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.JobPostingId);
                entity.HasIndex(e => e.ApplicantId);
                entity.HasIndex(e => new { e.JobPostingId, e.ApplicantId }).IsUnique();
            });

            // ApplicationStatusHistory configuration
            modelBuilder.Entity<ApplicationStatusHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromStatus).HasConversion<string>();
                entity.Property(e => e.ToStatus).HasConversion<string>();
                
                entity.HasOne(e => e.Application)
                    .WithMany(a => a.StatusHistory)
                    .HasForeignKey(e => e.ApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });

            // NotificationSettings configuration
            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<NotificationSettings>(e => e.UserId);
            });
        }
    }
}
