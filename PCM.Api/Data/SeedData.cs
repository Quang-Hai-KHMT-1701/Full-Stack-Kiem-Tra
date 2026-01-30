using Microsoft.AspNetCore.Identity;
using PCM.Api.Models.Identity;
using PCM.Api.Models.Core;
using PCM.Api.Models.Sports;
using PCM.Api.Enums;

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
                "Quản trị viên", "0901234567", 5.0);

            // Member 1
            var member1 = await CreateUserWithMember(
                userManager, context,
                "member@pcm.com", "Member@123", "Member",
                "Nguyễn Văn An", "0912345678", 3.5);

            // Member 2
            var member2 = await CreateUserWithMember(
                userManager, context,
                "player1@pcm.com", "Player@123", "Member",
                "Trần Thị Bình", "0923456789", 3.2);

            // Member 3
            var member3 = await CreateUserWithMember(
                userManager, context,
                "player2@pcm.com", "Player@123", "Member",
                "Lê Văn Cường", "0934567890", 2.8);

            // Member 4
            var member4 = await CreateUserWithMember(
                userManager, context,
                "player3@pcm.com", "Player@123", "Member",
                "Phạm Thị Dung", "0945678901", 3.0);

            // Member 5
            var member5 = await CreateUserWithMember(
                userManager, context,
                "player4@pcm.com", "Player@123", "Member",
                "Hoàng Văn Em", "0946789012", 2.5);

            // Member 6
            var member6 = await CreateUserWithMember(
                userManager, context,
                "player5@pcm.com", "Player@123", "Member",
                "Ngô Thị Phương", "0947890123", 3.3);

            // Member 7
            var member7 = await CreateUserWithMember(
                userManager, context,
                "player6@pcm.com", "Player@123", "Member",
                "Vũ Văn Giang", "0948901234", 2.9);

            // Member 8
            var member8 = await CreateUserWithMember(
                userManager, context,
                "player7@pcm.com", "Player@123", "Member",
                "Đặng Thị Hà", "0949012345", 3.1);

            // Member 9
            var member9 = await CreateUserWithMember(
                userManager, context,
                "player8@pcm.com", "Player@123", "Member",
                "Bùi Văn Inh", "0950123456", 2.7);

            // Member 10
            var member10 = await CreateUserWithMember(
                userManager, context,
                "player9@pcm.com", "Player@123", "Member",
                "Lý Thị Kim", "0951234567", 3.4);

            // Referee
            var refereeUser = await CreateUserWithMember(
                userManager, context,
                "referee@pcm.com", "Referee@123", "Referee",
                "Trọng tài CLB", "0956789012", 4.0);

            // Treasurer
            var treasurerUser = await CreateUserWithMember(
                userManager, context,
                "treasurer@pcm.com", "Treasurer@123", "Treasurer",
                "Thủ quỹ CLB", "0967890123", 3.8);

            // =========================
            // SEED COURTS
            // =========================
            if (!context.Courts.Any())
            {
                context.Courts.AddRange(
                    new Court { Name = "Sân A - Trong nhà", IsActive = true, Description = "Sân trong nhà, mát mẻ" },
                    new Court { Name = "Sân B - Trong nhà", IsActive = true, Description = "Sân trong nhà, ánh sáng tốt" },
                    new Court { Name = "Sân C - Ngoài trời", IsActive = true, Description = "Sân ngoài trời, view đẹp" },
                    new Court { Name = "Sân D - Ngoài trời", IsActive = false, Description = "Đang bảo trì" }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // SEED TRANSACTION CATEGORIES
            // =========================
            if (!context.TransactionCategories.Any())
            {
                context.TransactionCategories.AddRange(
                    new TransactionCategory { Name = "Phí thành viên", Type = "income", Description = "Thu phí hội viên hàng tháng", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Phí giải đấu", Type = "income", Description = "Thu phí tham gia giải đấu", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Tài trợ", Type = "income", Description = "Nhận tài trợ từ các đơn vị", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Bán hàng", Type = "income", Description = "Doanh thu từ bán đồ dùng", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Thuê sân", Type = "expense", Description = "Chi phí thuê sân tập/thi đấu", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Mua thiết bị", Type = "expense", Description = "Mua vợt, bóng, lưới...", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Giải thưởng", Type = "expense", Description = "Chi tiền thưởng giải đấu", IsActive = true, CreatedDate = DateTime.Now },
                    new TransactionCategory { Name = "Khác", Type = "expense", Description = "Chi phí khác", IsActive = true, CreatedDate = DateTime.Now }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // SEED TRANSACTIONS (Quỹ dương)
            // =========================
            if (!context.Transactions.Any())
            {
                context.Transactions.AddRange(
                    new Transaction { TransactionDate = DateTime.Now.AddDays(-30), Amount = 5000000, Description = "Quỹ khởi đầu CLB", Type = "income", CategoryId = 1, CategoryName = "Phí thành viên" },
                    new Transaction { TransactionDate = DateTime.Now.AddDays(-25), Amount = 2000000, Description = "Thu phí thành viên tháng 12", Type = "income", CategoryId = 1, CategoryName = "Phí thành viên" },
                    new Transaction { TransactionDate = DateTime.Now.AddDays(-20), Amount = -500000, Description = "Thuê sân tháng 12", Type = "expense", CategoryId = 5, CategoryName = "Thuê sân" },
                    new Transaction { TransactionDate = DateTime.Now.AddDays(-15), Amount = 1500000, Description = "Tài trợ giải cuối năm", Type = "income", CategoryId = 3, CategoryName = "Tài trợ" },
                    new Transaction { TransactionDate = DateTime.Now.AddDays(-10), Amount = -300000, Description = "Mua bóng pickleball mới", Type = "expense", CategoryId = 6, CategoryName = "Mua thiết bị" }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // SEED NEWS
            // =========================
            if (!context.News.Any())
            {
                context.News.AddRange(
                    new News { Title = "Khai trương CLB Pickleball Phố Núi", Content = "Chào mừng các bạn đến với CLB Pickleball Phố Núi - nơi giao lưu, rèn luyện và kết nối cộng đồng yêu thích bộ môn Pickleball!", Summary = "CLB chính thức ra mắt", IsPinned = true, Status = "Published", CreatedDate = DateTime.Now.AddDays(-7), CreatedBy = "Admin" },
                    new News { Title = "Giải đấu TeamBattle mùa xuân 2026", Content = "CLB tổ chức giải TeamBattle với format đội đấu đội. Chia 2 đội Team A và Team B, đấu theo thể thức Best of 5.", Summary = "Giải đấu hấp dẫn nhất năm", IsPinned = true, Status = "Published", CreatedDate = DateTime.Now.AddDays(-3), CreatedBy = "Admin" },
                    new News { Title = "Lịch tập luyện tuần mới", Content = "Lịch tập: Thứ 3-5-7 từ 6h-8h sáng và 17h-19h chiều tại Sân A và Sân B.", Summary = "Cập nhật lịch tập", IsPinned = false, Status = "Published", CreatedDate = DateTime.Now.AddDays(-1), CreatedBy = "Admin" }
                );
                await context.SaveChangesAsync();
            }

            // =========================
            // SEED CHALLENGE TEAMBATTLE ONGOING
            // =========================
            var allMembers = context.Members.ToList();
            if (!context.Challenges.Any() && allMembers.Count >= 10)
            {
                var teamBattleChallenge = new Challenge
                {
                    Title = "Giải TeamBattle Mùa Xuân 2026",
                    Type = "MiniGame",
                    GameMode = GameMode.TeamBattle,
                    Status = "Ongoing",
                    Config_TargetWins = 5,
                    CurrentScore_TeamA = 2,
                    CurrentScore_TeamB = 1,
                    EntryFee = 50000,
                    PrizePool = 600000,
                    MaxParticipants = 12,
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(14),
                    CreatedDate = DateTime.Now.AddDays(-7),
                    CreatedById = allMembers.First().Id
                };
                context.Challenges.Add(teamBattleChallenge);
                await context.SaveChangesAsync();

                // Add 12 participants chia team A/B
                var participants = new List<Participant>();
                for (int i = 0; i < Math.Min(12, allMembers.Count); i++)
                {
                    participants.Add(new Participant
                    {
                        ChallengeId = teamBattleChallenge.Id,
                        MemberId = allMembers[i].Id,
                        Team = i < 6 ? "TeamA" : "TeamB",
                        EntryFeePaid = true,
                        EntryFeeAmount = 50000,
                        Status = "Confirmed",
                        JoinedDate = DateTime.Now.AddDays(-5 + i)
                    });
                }
                context.Participants.AddRange(participants);
                await context.SaveChangesAsync();

                // Add 3 Matches trong Challenge này
                if (allMembers.Count >= 12)
                {
                    context.Matches.AddRange(
                        new Match
                        {
                            Date = DateTime.Now.AddDays(-2),
                            IsRanked = true,
                            ChallengeId = teamBattleChallenge.Id,
                            MatchFormat = 2, // Doubles
                            Team1_Player1Id = allMembers[0].Id,
                            Team1_Player2Id = allMembers[1].Id,
                            Team2_Player1Id = allMembers[6].Id,
                            Team2_Player2Id = allMembers[7].Id,
                            WinningSide = "Team1"
                        },
                        new Match
                        {
                            Date = DateTime.Now.AddDays(-1),
                            IsRanked = true,
                            ChallengeId = teamBattleChallenge.Id,
                            MatchFormat = 2, // Doubles
                            Team1_Player1Id = allMembers[2].Id,
                            Team1_Player2Id = allMembers[3].Id,
                            Team2_Player1Id = allMembers[8].Id,
                            Team2_Player2Id = allMembers[9].Id,
                            WinningSide = "Team1"
                        },
                        new Match
                        {
                            Date = DateTime.Now,
                            IsRanked = true,
                            ChallengeId = teamBattleChallenge.Id,
                            MatchFormat = 2, // Doubles
                            Team1_Player1Id = allMembers[4].Id,
                            Team1_Player2Id = allMembers[5].Id,
                            Team2_Player1Id = allMembers[10].Id,
                            Team2_Player2Id = allMembers[11].Id,
                            WinningSide = "Team2"
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Thêm 1 Challenge Open thông thường
                context.Challenges.Add(new Challenge
                {
                    Title = "Giải giao hữu cuối tuần",
                    Type = "Tournament",
                    GameMode = GameMode.FreeForAll,
                    Status = "Open",
                    EntryFee = 30000,
                    PrizePool = 0,
                    MaxParticipants = 8,
                    StartDate = DateTime.Now.AddDays(7),
                    CreatedDate = DateTime.Now,
                    CreatedById = allMembers.First().Id
                });
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
            string phoneNumber,
            double rankLevel = 2.0)
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
                    RankLevel = rankLevel,
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
