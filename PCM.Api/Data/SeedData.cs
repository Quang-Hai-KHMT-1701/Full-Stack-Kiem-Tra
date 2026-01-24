using Microsoft.AspNetCore.Identity;
using PCM.Api.Models.Identity;
using PCM.Api.Models.Core;
using PCM.Api.Models.Sports;

namespace PCM.Api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            // =========================
            // SEED USER (ADMIN)
            // =========================
            if (!userManager.Users.Any())
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@pcm.com",
                    Email = "admin@pcm.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin@123");
            }

            // =========================
            // SEED COURTS
            // =========================
            if (!context.Courts.Any())
            {
                context.Courts.AddRange(
                    new Court { Name = "Court A", IsActive = true },
                    new Court { Name = "Court B", IsActive = true }
                );
            }

            // =========================
            // SEED MEMBERS
            // =========================
            if (!context.Members.Any())
            {
                var user = userManager.Users.First();

                context.Members.Add(
                    new Member
                    {
                        FullName = "Nguyen Van A",
                        Email = "a@gmail.com",
                        PhoneNumber = "0123456789",
                        IsActive = true,
                        JoinDate = DateTime.Now,
                        UserId = user.Id,
                        TotalMatches = 0,
                        WinMatches = 0,
                        RankLevel = 1
                    }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}
