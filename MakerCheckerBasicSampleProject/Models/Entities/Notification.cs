using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; }
    
    [Required]
    public string Message { get; set; }
    
    [MaxLength(255)]
    public string Url { get; set; } // Redirect URL when clicked
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }

    // Notification type (approval, rejection, etc.)
    [MaxLength(50)]
    public string Type { get; set; }

    // Related entity (e.g., transaction id)
    [MaxLength(100)]
    public string RelatedEntityId { get; set; }
    
    [MaxLength(50)]
    public string RelatedEntityType { get; set; }
} 