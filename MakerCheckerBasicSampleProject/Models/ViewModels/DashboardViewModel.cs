using MakerCheckerBasicSampleProject.Models.Entities;

namespace MakerCheckerBasicSampleProject.Models.ViewModels;

// Dashboard View Model
public class DashboardViewModel
{
	public int PendingTransactionsCount { get; set; }
	public int ApprovedTransactionsCount { get; set; }
	public int RejectedTransactionsCount { get; set; }
	public int MyCreatedTransactionsCount { get; set; }
	public int PendingMyApprovalCount { get; set; }
	public List<string> UserRoles { get; set; }
	public List<Transaction> RecentTransactions { get; set; }
	public List<UserActivity> RecentActivity { get; set; }
}
