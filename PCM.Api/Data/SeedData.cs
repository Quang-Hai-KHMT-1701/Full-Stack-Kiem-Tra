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
            // SEED USERS WITH ROLES AND MEMBERS
            // =========================

            // Admin
            var adminUser = await CreateUserWithMember(
                userManager, context,
                "admin@pcm.com", "Admin@123", "Admin",
                "Quản trị viên", "0901234567");

            // Member 1
            var member1 = await CreateUserWithMember(
                userManager, context,
                "member@pcm.com", "Member@123", "Member",
                "Nguyễn Văn An", "0912345678");

            // Member 2
            var member2 = await CreateUserWithMember(
                userManager, context,
                "player1@pcm.com", "Player@123", "Member",
                "Trần Thị Bình", "0923456789");

            // Member 3
            var member3 = await CreateUserWithMember(
                userManager, context,
                "player2@pcm.com", "Player@123", "Member",
                "Lê Văn Cường", "0934567890");

            // Member 4
            var member4 = await CreateUserWithMember(
                userManager, context,
                "player3@pcm.com", "Player@123", "Member",
                "Phạm Thị Dung", "0945678901");

            // Referee
            var refereeUser = await CreateUserWithMember(
                userManager, context,
                "referee@pcm.com", "Referee@123", "Referee",
                "Trọng tài CLB", "0956789012");

            // Treasurer
            var treasurerUser = await CreateUserWithMember(
                userManager, context,
                "treasurer@pcm.com", "Treasurer@123", "Treasurer",
                "Thủ quỹ CLB", "0967890123");

            // =========================
            // SEED COURTS
            // =========================
            if (!context.Courts.Any())
            {
                context.Courts.AddRange(
                    new Court { Name = "Sân A - Trong nhà", IsActive = true },
                    new Court { Name = "Sân B - Trong nhà", IsActive = true },
                    new Court { Name = "Sân C - Ngoài trời", IsActive = true },
                    new Court { Name = "Sân D - Ngoài trời", IsActive = false }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // SEED CHALLENGES
            // =========================
            if (!context.Challenges.Any())
            {
                var firstMember = context.Members.FirstOrDefault();
                context.Challenges.AddRange(
                    new Challenge
                    {
                        Title = "Giải giao hữu cuối tuần",
                        EntryFee = 50000,
                        PrizePool = 0,
                        MaxParticipants = 8,
                        Status = "Open",
                        StartDate = DateTime.Now.AddDays(3),
                        CreatedDate = DateTime.Now,
                        CreatedById = firstMember?.Id ?? 1
                    },
                    new Challenge
                    {
                        Title = "Giải đấu mùa xuân 2026",
                        EntryFee = 100000,
                        PrizePool = 500000,
                        MaxParticipants = 16,
                        Status = "Open",
                        StartDate = DateTime.Now.AddDays(14),
                        CreatedDate = DateTime.Now,
                        CreatedById = firstMember?.Id ?? 1
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser?> CreateUserWithMember(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string password,
            string role,
            string fullName,
            string phoneNumber)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, password);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            // Tạo Member profile nếu chưa có
            var existingMember = context.Members.FirstOrDefault(m => m.UserId == user.Id);
            if (existingMember == null)
            {
                var member = new Member
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    UserId = user.Id,
                    JoinDate = DateTime.Now,
                    IsActive = true,
                    RankLevel = role == "Admin" ? 5.0 : 2.0,
                    TotalMatches = 0,
                    WinMatches = 0,
                    CreatedDate = DateTime.Now
                };
                context.Members.Add(member);
                await context.SaveChangesAsync();
            }

            return user;
        }
    }
}
