using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class TransactionLog
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	public Guid TransactionId { get; set; }

	[ForeignKey("TransactionId")]
	public Transaction Transaction { get; set; }

	[Required]
	[MaxLength(50)]
	public string Action { get; set; }

	[Required]
	public string UserId { get; set; }

	[ForeignKey("UserId")]
	public ApplicationUser User { get; set; }

	[Required]
	public DateTime Timestamp { get; set; }

	[Required]
	public string IpAddress { get; set; }

	[MaxLength(1000)]
	public string Detail { get; set; }
}
