using MakerCheckerBasicSampleProject.Models.Entities;

namespace MakerCheckerBasicSampleProject.Models.ViewModels;

// Transaction Details View Model
public class TransactionDetailsViewModel
{
	public Transaction Transaction { get; set; }
	public List<TransactionLog> Logs { get; set; }
	public List<TransactionApproval> Approvals { get; set; }
	public ApprovalWorkflow Workflow { get; set; }
	public int CurrentApprovalLevel { get; set; }
	public int TotalApprovalLevels { get; set; }
	public bool CanApprove { get; set; }
	public bool CanReject { get; set; }
}
