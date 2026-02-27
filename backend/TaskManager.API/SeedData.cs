using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.API
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(ApplicationDbContext context, IConfiguration configuration)
        {
            if (!await context.Users.AnyAsync())
            {
                var defaultUserEmail = "admin@taskmanager.local";
                var defaultUserName = "Administrator";
                var defaultPassword = "Admin@123";

                var passwordSalt = configuration.GetSection("Auth")["PasswordSalt"] ?? "DefaultSalt";

                var hash = HashPassword(defaultPassword, passwordSalt);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = defaultUserName,
                    Email = defaultUserEmail,
                    PasswordHash = hash,
                    CreatedOn = DateTime.UtcNow
                };

                await context.Users.AddAsync(user);

                var firstTask = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Initial seeded task",
                    Description = "This is a seeded task to validate the system.",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Status = "Pending",
                    Remarks = "Seed data",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    CreatedByUserId = user.Id,
                    UpdatedByUserId = user.Id
                };

                await context.Tasks.AddAsync(firstTask);

                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}

