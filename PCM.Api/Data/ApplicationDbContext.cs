using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PCM.Api.Models.Core;
using PCM.Api.Models.Identity;
using PCM.Api.Models.Sports;

namespace PCM.Api.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionCategory> TransactionCategories { get; set; }
        public DbSet<News> News { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================
            // TABLE NAMING
            // =========================
            builder.Entity<Member>().ToTable("123_Members");
            builder.Entity<Court>().ToTable("123_Courts");
            builder.Entity<Booking>().ToTable("123_Bookings");
            builder.Entity<Challenge>().ToTable("123_Challenges");
            builder.Entity<Match>().ToTable("123_Matches");
            builder.Entity<Participant>().ToTable("123_Participants");
            builder.Entity<Transaction>().ToTable("123_Transactions");
            builder.Entity<TransactionCategory>().ToTable("123_TransactionCategories");
            builder.Entity<News>().ToTable("123_News");

            // =========================
            // DECIMAL PRECISION
            // =========================
            builder.Entity<Challenge>()
                .Property(x => x.EntryFee)
                .HasPrecision(18, 2);

            builder.Entity<Challenge>()
                .Property(x => x.PrizePool)
                .HasPrecision(18, 2);

            builder.Entity<Participant>()
                .Property(x => x.EntryFeeAmount)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            // =========================
            // FIX MULTIPLE CASCADE PATHS (MATCH)
            // =========================
            builder.Entity<Match>()
                .HasOne(m => m.Team1_Player1)
                .WithMany()
                .HasForeignKey(m => m.Team1_Player1Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Match>()
                .HasOne(m => m.Team1_Player2)
                .WithMany()
                .HasForeignKey(m => m.Team1_Player2Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Match>()
                .HasOne(m => m.Team2_Player1)
                .WithMany()
                .HasForeignKey(m => m.Team2_Player1Id)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Match>()
                .HasOne(m => m.Team2_Player2)
                .WithMany()
                .HasForeignKey(m => m.Team2_Player2Id)
                .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // FIX CASCADE PATHS (PARTICIPANT)
            // =========================
            builder.Entity<Participant>()
            .HasOne(p => p.Member)
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Participant>()
                .HasOne(p => p.Challenge)
                .WithMany(c => c.Participants)
                .HasForeignKey(p => p.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
