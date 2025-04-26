using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class TransactionApproval
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid TransactionId { get; set; }
    
    [ForeignKey("TransactionId")]
    public Transaction Transaction { get; set; }
    
    // From Configuration version
    [Required]
    public int ApprovalLevelId { get; set; }

    [ForeignKey("ApprovalLevelId")]
    public ApprovalLevel ApprovalLevel { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
    
    [Required]
    public DateTime ApprovedAt { get; set; }
    
    [Required]
    public string IpAddress { get; set; }
    
    [MaxLength(1000)]
    public string Comments { get; set; }
    
    [Required]
    public bool IsApproved { get; set; }
} 