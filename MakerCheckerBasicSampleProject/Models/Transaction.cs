using MakerCheckerBasicSampleProject.Models.State;

namespace MakerCheckerBasicSampleProject.Models;

public class Transaction
{
	public Guid Id { get; set; }
	public string Description { get; set; }
	public decimal Amount { get; set; }
	public TransactionState State { get; set; }
	public string CreatedBy { get; set; }
	public DateTime CreatedAt { get; set; }
	public string ApprovedBy { get; set; }
	public DateTime? ApprovedAt { get; set; }
	public string RejectedBy { get; set; }
	public DateTime? RejectedAt { get; set; }
	public string IpAddress { get; set; }
}
