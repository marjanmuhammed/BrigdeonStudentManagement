using Bridgeon.Models;
using Bridgeon.Models.Attendence;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Bridgeon.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        // --- DbSets ---
        public DbSet<User> Users { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }

        // 🔹 New Attendance & LeaveRequest tables
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

      


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 🔹 User - Profile: One-to-One Relationship
            builder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 User - RefreshToken: One-to-Many Relationship
            builder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Unique Email constraint
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 🔹 Default values
            builder.Entity<Profile>()
                .Property(p => p.Week)
                .HasDefaultValue(0);

            builder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");

            // =============================
            // 🔹 Attendance Configurations
            // =============================

            // One attendance per user per date (avoid duplicates)
            builder.Entity<Attendance>()
                .HasIndex(a => new { a.UserId, a.Date })
                .IsUnique();

            // =============================
            // 🔹 LeaveRequest Configurations
            // =============================
            builder.Entity<LeaveRequest>()
                .HasIndex(l => new { l.UserId, l.Date });


            // Configure one-to-many self-reference for mentors
            // ===================================================
            builder.Entity<User>()
                .HasOne(u => u.Mentor)       // each user has one mentor
                .WithMany(m => m.Mentees)    // mentor has many mentees
                .HasForeignKey(u => u.MentorId)
                .OnDelete(DeleteBehavior.NoAction); // ❗ no cascade delete (safe)



        }
    }
}
