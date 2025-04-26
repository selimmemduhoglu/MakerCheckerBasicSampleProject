using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class UserSetting
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;

    // Dashboard settings
    [MaxLength(50)]
    public string DefaultDashboard { get; set; } = "default";

    // Theme settings
    [MaxLength(20)]
    public string Theme { get; set; } = "light";

    // Timezone settings
    [MaxLength(50)]
    public string TimeZone { get; set; } = "UTC";
} 