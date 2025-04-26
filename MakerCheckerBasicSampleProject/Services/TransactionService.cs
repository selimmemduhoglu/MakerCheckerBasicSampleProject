using MakerCheckerBasicSampleProject.Models;
using MakerCheckerBasicSampleProject.Models.Log;
using MakerCheckerBasicSampleProject.Models.State;
using MakerCheckerBasicSampleProject.StateFilter;

namespace MakerCheckerBasicSampleProject.Services;

// Service for transaction management
public class TransactionService
{
	private static List<Transaction> _transactions = new List<Transaction>();
	private static List<LogEntry> _logs = new List<LogEntry>();
	private static Dictionary<string, string> _userRoles = new Dictionary<string, string>();

	// Initialize with two users
	static TransactionService()
	{
		// Initially set user1 as Maker and user2 as Checker
		_userRoles["user1"] = "Maker";
		_userRoles["user2"] = "Checker";
	}

	// Create new transaction
	public Transaction CreateTransaction(string description, decimal amount, string username, string ipAddress)
	{
		// Check if user has Maker role
		if (_userRoles[username] != "Maker")
		{
			throw new UnauthorizedAccessException("Only users with Maker role can create transactions");
		}

		var transaction = new Transaction
		{
			Id = Guid.NewGuid(),
			Description = description,
			Amount = amount,
			State = TransactionState.PendingApproval,
			CreatedBy = username,
			CreatedAt = DateTime.Now,
			IpAddress = ipAddress
		};

		_transactions.Add(transaction);

		// Log the action
		LogAction(transaction.Id, "Create", username, ipAddress, $"Transaction created: {description}, Amount: {amount}");

		return transaction;
	}

	// Approve transaction
	public Transaction ApproveTransaction(Guid transactionId, string username, string ipAddress)
	{
		// Check if user has Checker role
		if (_userRoles[username] != "Checker")
		{
			throw new UnauthorizedAccessException("Only users with Checker role can approve transactions");
		}

		var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);

		if (transaction == null)
		{
			throw new KeyNotFoundException("Transaction not found");
		}

		// Cannot approve own transaction
		if (transaction.CreatedBy == username)
		{
			throw new InvalidOperationException("Cannot approve your own transaction");
		}

		// Check if transaction is in pending state
		if (transaction.State != TransactionState.PendingApproval)
		{
			throw new InvalidOperationException("Only pending transactions can be approved");
		}

		transaction.State = TransactionState.Approved;
		transaction.ApprovedBy = username;
		transaction.ApprovedAt = DateTime.Now;

		// Swap roles after approval
		SwapRoles();

		// Log the action
		LogAction(transaction.Id, "Approve", username, ipAddress, $"Transaction approved: {transaction.Description}");

		return transaction;
	}

	// Reject transaction
	public Transaction RejectTransaction(Guid transactionId, string username, string ipAddress, string reason)
	{
		// Check if user has Checker role
		if (_userRoles[username] != "Checker")
		{
			throw new UnauthorizedAccessException("Only users with Checker role can reject transactions");
		}

		var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);

		if (transaction == null)
		{
			throw new KeyNotFoundException("Transaction not found");
		}

		// Cannot reject own transaction
		if (transaction.CreatedBy == username)
		{
			throw new InvalidOperationException("Cannot reject your own transaction");
		}

		// Check if transaction is in pending state
		if (transaction.State != TransactionState.PendingApproval)
		{
			throw new InvalidOperationException("Only pending transactions can be rejected");
		}

		transaction.State = TransactionState.Rejected;
		transaction.RejectedBy = username;
		transaction.RejectedAt = DateTime.Now;

		// Swap roles after rejection
		SwapRoles();

		// Log the action
		LogAction(transaction.Id, "Reject", username, ipAddress, $"Transaction rejected: {transaction.Description}, Reason: {reason}");

		return transaction;
	}

	// Swap roles between users
	private void SwapRoles()
	{
		if (_userRoles["user1"] == "Maker")
		{
			_userRoles["user1"] = "Checker";
			_userRoles["user2"] = "Maker";
		}
		else
		{
			_userRoles["user1"] = "Maker";
			_userRoles["user2"] = "Checker";
		}
	}

	// Log an action
	private void LogAction(Guid transactionId, string action, string username, string ipAddress, string detail)
	{
		var log = new LogEntry
		{
			Id = Guid.NewGuid(),
			TransactionId = transactionId,
			Action = action,
			Username = username,
			Timestamp = DateTime.Now,
			IpAddress = ipAddress,
			Detail = detail
		};

		_logs.Add(log);
	}

	// Get transactions based on filter
	public List<Transaction> GetTransactions(int filterId, string username)
	{
		var filters = DefaultStateFilterFactory.GetStateFilter();
		var filter = filters.Keys.FirstOrDefault(f => f.Id == filterId);

		if (filter == null)
		{
			return _transactions;
		}

		var conditionPair = filters[filter];

		switch (filter.Name)
		{
			case "Active Records":
				return _transactions.Where(t => t.State == TransactionState.Approved || t.State == TransactionState.PendingApproval).ToList();
			case "All Records":
				return _transactions;
			case "Unverified Records":
				return _transactions.Where(t => t.State == TransactionState.PendingApproval).ToList();
			case "Verified Records":
				return _transactions.Where(t => t.State == TransactionState.Approved).ToList();
			case "Rejected Records":
				return _transactions.Where(t => t.State == TransactionState.Rejected).ToList();
			default:
				return _transactions;
		}
	}

	// Get current user role
	public string GetUserRole(string username)
	{
		return _userRoles.ContainsKey(username) ? _userRoles[username] : null;
	}

	// Get transaction logs
	public List<LogEntry> GetLogs(Guid? transactionId = null)
	{
		if (transactionId.HasValue)
		{
			return _logs.Where(l => l.TransactionId == transactionId.Value).ToList();
		}

		return _logs;
	}
}
