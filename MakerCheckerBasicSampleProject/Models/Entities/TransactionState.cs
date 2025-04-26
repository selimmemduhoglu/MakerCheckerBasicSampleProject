namespace MakerCheckerBasicSampleProject.Models.Entities;

public enum TransactionState
{
	PendingApproval = 0,
	Rejected = 1,
	Approved = 2,
	PendingMultipleApproval = 3, // New state for multi-level approval
	PartiallyApproved = 4        // New state for multi-level approval
}
