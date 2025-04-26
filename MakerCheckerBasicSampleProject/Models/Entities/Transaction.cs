using MakerCheckerBasicSampleProject.Models.State;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class Transaction
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(500)]
	public string Description { get; set; }

	[Required]
	[Column(TypeName = "decimal(18,2)")]
	public decimal Amount { get; set; }

	[Required]
	public TransactionState State { get; set; }

	[Required]
	public string CreatedById { get; set; }

	[ForeignKey("CreatedById")]
	public ApplicationUser CreatedBy { get; set; }

	[Required]
	public DateTime CreatedAt { get; set; }

	public string ApprovedById { get; set; }

	[ForeignKey("ApprovedById")]
	public ApplicationUser ApprovedBy { get; set; }

	public DateTime? ApprovedAt { get; set; }

	public string RejectedById { get; set; }

	[ForeignKey("RejectedById")]
	public ApplicationUser RejectedBy { get; set; }

	public DateTime? RejectedAt { get; set; }

	[Required]
	public string IpAddress { get; set; }

	// Navigation properties
	public ICollection<TransactionLog> Logs { get; set; }
	public ICollection<TransactionApproval> Approvals { get; set; }
}
