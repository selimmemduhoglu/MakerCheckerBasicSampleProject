using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class UserActivity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Activity { get; set; }
    
    [MaxLength(50)]
    public string Module { get; set; }
    
    [MaxLength(50)]
    public string IpAddress { get; set; }
    
    [MaxLength(255)]
    public string UserAgent { get; set; }
    
    public bool IsSuccessful { get; set; }
    
    public string Details { get; set; }
} 