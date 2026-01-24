using PCM.Api.Models.Core;
using PCM.Api.Models.Sports;

namespace PCM.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Courts.Any())
            {
                context.Courts.AddRange(
                    new Court { Name = "San 1", IsActive = true },
                    new Court { Name = "San 2", IsActive = true }
                );
            }

            if (!context.Members.Any())
            {
                context.Members.AddRange(
                    new Member
                    {
                        FullName = "Nguyen Van A",
                        Email = "a@gmail.com",
                        PhoneNumber = "0123456789",
                        UserId = "demo-user-1",
                        JoinDate = DateTime.Now,
                        IsActive = true
                    },
                    new Member
                    {
                        FullName = "Tran Van B",
                        Email = "b@gmail.com",
                        PhoneNumber = "0987654321",
                        UserId = "demo-user-2",
                        JoinDate = DateTime.Now,
                        IsActive = true
                    }
                );
            }

            if (!context.Challenges.Any())
            {
                context.Challenges.Add(
                    new Challenge
                    {
                        Title = "Keo giao huu cuoi tuan",
                        EntryFee = 50000,
                        PrizePool = 0,
                        MaxParticipants = 8,
                        Status = "Open",
                        StartDate = DateTime.Now.AddDays(1)
                    }
                );
            }

            context.SaveChanges();
        }
    }
}
