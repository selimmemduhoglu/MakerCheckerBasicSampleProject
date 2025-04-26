using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class ApprovalLevel
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int WorkflowId { get; set; }
    
    [ForeignKey("WorkflowId")]
    public ApprovalWorkflow Workflow { get; set; }
    
    [Required]
    public int Level { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    public string RoleId { get; set; }
    
    [ForeignKey("RoleId")]
    public IdentityRole Role { get; set; }
} 