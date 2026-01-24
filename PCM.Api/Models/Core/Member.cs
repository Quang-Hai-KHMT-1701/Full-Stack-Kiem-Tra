using PCM.Api.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Member
{
    public int Id { get; set; }

    public string FullName { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime JoinDate { get; set; }

    public double RankLevel { get; set; }

    public bool IsActive { get; set; }

    // =====================
    // FK
    // =====================
    [Required]
    public string UserId { get; set; }

    // =====================
    // Navigation (KHÔNG VALIDATE, KHÔNG POST)
    // =====================
    [JsonIgnore]
    public ApplicationUser? User { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int TotalMatches { get; set; }
    public int WinMatches { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
}
