using MakerCheckerBasicSampleProject.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace MakerCheckerBasicSampleProject.Models;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
	public string Department { get; set; }
	public string Position { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.Now;
	public DateTime? LastLoginAt { get; set; }
	public bool IsActive { get; set; } = true;

	// Navigation properties
	public ICollection<Transaction> CreatedTransactions { get; set; }
	public ICollection<Transaction> ApprovedTransactions { get; set; }
	public ICollection<Transaction> RejectedTransactions { get; set; }
	public ICollection<TransactionLog> Logs { get; set; }
	public ICollection<TransactionApproval> Approvals { get; set; }
	public ICollection<Notification> Notifications { get; set; }
}
