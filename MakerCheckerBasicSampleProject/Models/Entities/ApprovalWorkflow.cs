using System.ComponentModel.DataAnnotations;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class ApprovalWorkflow
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    public int RequiredApprovals { get; set; }
    
    [MaxLength(255)]
    public string Description { get; set; }
    
    [Required]
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<ApprovalLevel> ApprovalLevels { get; set; }
} 