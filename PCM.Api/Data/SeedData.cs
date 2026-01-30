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
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // =========================
            // SEED ROLES
            // =========================
            string[] roles = { "Admin", "Member", "Referee", "Treasurer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // =========================
            // SEED USERS WITH ROLES
            // =========================
            // Admin
            var admin = await userManager.FindByEmailAsync("admin@pcm.com");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin@pcm.com",
                    Email = "admin@pcm.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
            }
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Member
            var member = await userManager.FindByEmailAsync("member@pcm.com");
            if (member == null)
            {
                member = new ApplicationUser
                {
                    UserName = "member@pcm.com",
                    Email = "member@pcm.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(member, "Member@123");
            }
            if (!await userManager.IsInRoleAsync(member, "Member"))
            {
                await userManager.AddToRoleAsync(member, "Member");
            }

            // Referee
            var referee = await userManager.FindByEmailAsync("referee@pcm.com");
            if (referee == null)
            {
                referee = new ApplicationUser
                {
                    UserName = "referee@pcm.com",
                    Email = "referee@pcm.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(referee, "Referee@123");
            }
            if (!await userManager.IsInRoleAsync(referee, "Referee"))
            {
                await userManager.AddToRoleAsync(referee, "Referee");
            }

            // Treasurer
            var treasurer = await userManager.FindByEmailAsync("treasurer@pcm.com");
            if (treasurer == null)
            {
                treasurer = new ApplicationUser
                {
                    UserName = "treasurer@pcm.com",
                    Email = "treasurer@pcm.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(treasurer, "Treasurer@123");
            }
            if (!await userManager.IsInRoleAsync(treasurer, "Treasurer"))
            {
                await userManager.AddToRoleAsync(treasurer, "Treasurer");
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
