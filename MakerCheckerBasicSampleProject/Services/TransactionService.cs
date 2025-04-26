//using MakerCheckerBasicSampleProject.Config.Configuration;
//using MakerCheckerBasicSampleProject.Configuration;
//using MakerCheckerBasicSampleProject.Configuration.Ef;
//using MakerCheckerBasicSampleProject.Models;
//using MakerCheckerBasicSampleProject.Models.Entities;
//using MakerCheckerBasicSampleProject.SignalR;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

namespace MakerCheckerBasicSampleProject.Services;

public class TransactionService
{
	//		private readonly ApplicationDbContext _context;
	//		private readonly UserManager<ApplicationUser> _userManager;
	//		private readonly IUserActivityService _userActivityService;
	//		private readonly NotificationService _notificationService;
	//		private readonly IHttpContextAccessor _httpContextAccessor;

	//		public TransactionService(
	//			ApplicationDbContext context,
	//			UserManager<ApplicationUser> userManager,
	//			IUserActivityService userActivityService,
	//			NotificationService notificationService,
	//			IHttpContextAccessor httpContextAccessor)
	//		{
	//			_context = context;
	//			_userManager = userManager;
	//			_userActivityService = userActivityService;
	//			_notificationService = notificationService;
	//			_httpContextAccessor = httpContextAccessor;
	//		}

	//		// Create new transaction
	//		public async Task<Transaction> CreateTransactionAsync(string description, decimal amount, string userId, int workflowId = 1)
	//		{
	//			// Get user info
	//			var user = await _userManager.FindByIdAsync(userId);
	//			if (user == null)
	//			{
	//				throw new ArgumentException("User not found", nameof(userId));
	//			}

	//			// Check if user has maker role
	//			var isMaker = await _userManager.IsInRoleAsync(user, "Maker");
	//			if (!isMaker)
	//			{
	//				throw new UnauthorizedAccessException("Only users with Maker role can create transactions");
	//			}

	//			// Get workflow
	//			var workflow = await _context.ApprovalWorkflows
	//				.Include(w => w.Levels.OrderBy(l => l.Level))
	//				.FirstOrDefaultAsync(w => w.Id == workflowId && w.IsActive);

	//			if (workflow == null)
	//			{
	//				throw new ArgumentException("Invalid or inactive workflow", nameof(workflowId));
	//			}

	//			// Get client IP address
	//			var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

	//			// Create transaction
	//			var transaction = new Transaction
	//			{
	//				Id = Guid.NewGuid(),
	//				Description = description,
	//				Amount = amount,
	//				State = workflow.RequiredApprovals > 1 ? TransactionState.PendingMultipleApproval : TransactionState.PendingApproval,
	//				CreatedById = userId,
	//				CreatedAt = DateTime.Now,
	//				IpAddress = ipAddress
	//			};

	//			_context.Transactions.Add(transaction);

	//			// Log the action
	//			await LogActionAsync(transaction.Id, "Create", userId, ipAddress, $"Transaction created: {description}, Amount: {amount}");

	//			// Send notifications to approvers (checkers)
	//			await SendApprovalNotificationsAsync(transaction, workflow.Levels.First());

	//			await _context.SaveChangesAsync();

	//			// Log user activity
	//			await _userActivityService.LogActivityAsync(
	//				userId,
	//				"Create Transaction",
	//				"Transactions",
	//				ipAddress,
	//				_httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
	//				true,
	//				$"Transaction ID: {transaction.Id}, Amount: {amount}"
	//			);

	//			return transaction;
	//		}

	//		// Approve transaction
	//		public async Task<Transaction> ApproveTransactionAsync(Guid transactionId, string userId, string comment = null)
	//		{
	//			// Get user info
	//			var user = await _userManager.FindByIdAsync(userId);
	//			if (user == null)
	//			{
	//				throw new ArgumentException("User not found", nameof(userId));
	//			}

	//			// Get transaction
	//			var transaction = await _context.Transactions
	//				.Include(t => t.CreatedBy)
	//				.FirstOrDefaultAsync(t => t.Id == transactionId);

	//			if (transaction == null)
	//			{
	//				throw new KeyNotFoundException("Transaction not found");
	//			}

	//			// Cannot reject own transaction
	//			if (transaction.CreatedById == userId)
	//			{
	//				throw new InvalidOperationException("Cannot reject your own transaction");
	//			}

	//			// Check if transaction is in pending state
	//			if (transaction.State != TransactionState.PendingApproval &&
	//				transaction.State != TransactionState.PendingMultipleApproval &&
	//				transaction.State != TransactionState.PartiallyApproved)
	//			{
	//				throw new InvalidOperationException("Only pending transactions can be rejected");
	//			}

	//			// Get client IP address
	//			var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

	//			// Update transaction state
	//			transaction.State = TransactionState.Rejected;
	//			transaction.RejectedById = userId;
	//			transaction.RejectedAt = DateTime.Now;

	//			// Record the rejection (as a negative approval)
	//			// Find the current workflow level
	//			int workflowId = 1; // Default
	//			int currentLevel = 1;

	//			// Try to determine from previous approvals
	//			var existingApprovals = await _context.TransactionApprovals
	//				.Include(a => a.ApprovalLevel)
	//				.Where(a => a.TransactionId == transactionId)
	//				.OrderByDescending(a => a.ApprovalLevel.Level)
	//				.ToListAsync();

	//			if (existingApprovals.Any())
	//			{
	//				var lastApproval = existingApprovals.First();
	//				workflowId = lastApproval.ApprovalLevel.WorkflowId;
	//				currentLevel = lastApproval.ApprovalLevel.Level + 1;
	//			}

	//			// Get the workflow
	//			var workflow = await _context.ApprovalWorkflows
	//				.Include(w => w.Levels.OrderBy(l => l.Level))
	//				.FirstOrDefaultAsync(w => w.Id == workflowId);

	//			if (workflow != null)
	//			{
	//				// Get appropriate level
	//				var approvalLevel = workflow.Levels.FirstOrDefault(l => l.Level == currentLevel) ??
	//								   workflow.Levels.FirstOrDefault(l => l.Level == 1);

	//				if (approvalLevel != null)
	//				{
	//					// Record rejection
	//					var rejection = new TransactionApproval
	//					{
	//						Id = Guid.NewGuid(),
	//						TransactionId = transactionId,
	//						ApprovalLevelId = approvalLevel.Id,
	//						ApproverId = userId,
	//						ApprovedAt = DateTime.Now,
	//						IpAddress = ipAddress,
	//						Comment = reason,
	//						IsApproved = false // Rejection
	//					};

	//					_context.TransactionApprovals.Add(rejection);
	//				}
	//			}

	//			// Log the action
	//			await LogActionAsync(transaction.Id, "Reject", userId, ipAddress,
	//				$"Transaction rejected by {user.FullName}, Reason: {reason}");

	//			// Send notification to maker
	//			await _notificationService.CreateNotificationAsync(
	//				transaction.CreatedById,
	//				"Transaction Rejected",
	//				$"Your transaction {transaction.Description} for {transaction.Amount:C} has been rejected. Reason: {reason}",
	//				"Rejection",
	//				$"/Transaction/Details/{transaction.Id}",
	//				transaction.Id.ToString(),
	//				"Transaction"
	//			);

	//			await _context.SaveChangesAsync();

	//			// Log user activity
	//			await _userActivityService.LogActivityAsync(
	//				userId,
	//				"Reject Transaction",
	//				"Transactions",
	//				ipAddress,
	//				_httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString(),
	//				true,
	//				$"Transaction ID: {transaction.Id}, Reason: {reason}"
	//			);

	//			return transaction;
	//		}

	//		// Get transactions based on filter
	//		public async Task<List<Transaction>> GetTransactionsAsync(int filterId, string userId, int page = 1, int pageSize = 10)
	//		{
	//			IQueryable<Transaction> query = _context.Transactions
	//				.Include(t => t.CreatedBy)
	//				.Include(t => t.ApprovedBy)
	//				.Include(t => t.RejectedBy)
	//				.AsQueryable();

	//			// Apply filter
	//			switch (filterId)
	//			{
	//				case 1: // Active Records
	//					query = query.Where(t => t.State == TransactionState.Approved ||
	//											t.State == TransactionState.PendingApproval ||
	//											t.State == TransactionState.PendingMultipleApproval ||
	//											t.State == TransactionState.PartiallyApproved);
	//					break;
	//				case 3: // Unverified Records
	//					query = query.Where(t => t.State == TransactionState.PendingApproval ||
	//											t.State == TransactionState.PendingMultipleApproval ||
	//											t.State == TransactionState.PartiallyApproved);
	//					break;
	//				case 4: // Verified Records
	//					query = query.Where(t => t.State == TransactionState.Approved);
	//					break;
	//				case 5: // Rejected Records
	//					query = query.Where(t => t.State == TransactionState.Rejected);
	//					break;
	//				// Additional filters
	//				case 6: // My Created Transactions
	//					query = query.Where(t => t.CreatedById == userId);
	//					break;
	//				case 7: // My Approved Transactions
	//					query = query.Where(t => t.ApprovedById == userId);
	//					break;
	//				case 8: // My Rejected Transactions
	//					query = query.Where(t => t.RejectedById == userId);
	//					break;
	//				case 9: // Pending My Approval
	//						// Get user roles
	//					var user = await _userManager.FindByIdAsync(userId);
	//					var userRoles = await _userManager.GetRolesAsync(user);

	//					// Find approval levels associated with user roles
	//					var roleIds = _context.Roles
	//						.Where(r => userRoles.Contains(r.Name))
	//						.Select(r => r.Id)
	//						.ToList();

	//					var approvalLevelIds = _context.ApprovalLevels
	//						.Where(al => roleIds.Contains(al.RoleId))
	//						.Select(al => al.Id)
	//						.ToList();

	//					// Find transactions that:
	//					// 1. Are in appropriate pending state
	//					// 2. Are not created by this user
	//					// 3. Either have no approvals yet (for first level) or have approvals up to previous level
	//					query = query.Where(t =>
	//						(t.State == TransactionState.PendingApproval ||
	//						 t.State == TransactionState.PendingMultipleApproval ||
	//						 t.State == TransactionState.PartiallyApproved) &&
	//						t.CreatedById != userId
	//					);

	//					// Filter more specifically based on approval levels
	//					// This part is complex and depends on your exact workflow logic
	//					break;
	//				default: // All Records (case 2)
	//					break;
	//			}

	//			// Apply paging
	//			return await query
	//				.OrderByDescending(t => t.CreatedAt)
	//				.Skip((page - 1) * pageSize)
	//				.Take(pageSize)
	//				.ToListAsync();
	//		}

	//		// Get transaction details
	//		public async Task<Transaction> GetTransactionAsync(Guid transactionId)
	//		{
	//			return await _context.Transactions
	//				.Include(t => t.CreatedBy)
	//				.Include(t => t.ApprovedBy)
	//				.Include(t => t.RejectedBy)
	//				.Include(t => t.Logs)
	//					.ThenInclude(l => l.User)
	//				.Include(t => t.Approvals)
	//					.ThenInclude(a => a.Approver)
	//				.Include(t => t.Approvals)
	//					.ThenInclude(a => a.ApprovalLevel)
	//				.FirstOrDefaultAsync(t => t.Id == transactionId);
	//		}

	//		// Get transaction logs
	//		public async Task<List<TransactionLog>> GetTransactionLogsAsync(Guid transactionId)
	//		{
	//			return await _context.TransactionLogs
	//				.Include(l => l.User)
	//				.Include(l => l.Transaction)
	//				.Where(l => l.TransactionId == transactionId)
	//				.OrderByDescending(l => l.Timestamp)
	//				.ToListAsync();
	//		}

	//		// Get transaction approvals
	//		public async Task<List<TransactionApproval>> GetTransactionApprovalsAsync(Guid transactionId)
	//		{
	//			return await _context.TransactionApprovals
	//				.Include(a => a.Approver)
	//				.Include(a => a.ApprovalLevel)
	//				.Where(a => a.TransactionId == transactionId)
	//				.OrderBy(a => a.ApprovalLevel.Level)
	//				.ThenBy(a => a.ApprovedAt)
	//				.ToListAsync();
	//		}

	//		// Get approval workflows
	//		public async Task<List<ApprovalWorkflow>> GetApprovalWorkflowsAsync()
	//		{
	//			return await _context.ApprovalWorkflows
	//				.Include(w => w.Levels.OrderBy(l => l.Level))
	//					.ThenInclude(l => l.Role)
	//				.Where(w => w.IsActive)
	//				.OrderBy(w => w.Name)
	//				.ToListAsync();
	//		}

	//		// Get transaction approval status
	//		public async Task<Dictionary<string, object>> GetTransactionApprovalStatusAsync(Guid transactionId)
	//		{
	//			var transaction = await GetTransactionAsync(transactionId);
	//			if (transaction == null)
	//			{
	//				throw new KeyNotFoundException("Transaction not found");
	//			}

	//			var approvals = await GetTransactionApprovalsAsync(transactionId);

	//			// Determine workflow
	//			int workflowId = 1; // Default to standard
	//			if (approvals.Any())
	//			{
	//				workflowId = approvals.First().ApprovalLevel.WorkflowId;
	//			}

	//			var workflow = await _context.ApprovalWorkflows
	//				.Include(w => w.Levels.OrderBy(l => l.Level))
	//					.ThenInclude(l => l.Role)
	//				.FirstOrDefaultAsync(w => w.Id == workflowId);

	//			if (workflow == null)
	//			{
	//				workflow = await _context.ApprovalWorkflows
	//					.Include(w => w.Levels.OrderBy(l => l.Level))
	//						.ThenInclude(l => l.Role)
	//					.FirstOrDefaultAsync(w => w.Id == 1); // Fallback to standard
	//			}

	//			var result = new Dictionary<string, object>
	//			{
	//				["Transaction"] = transaction,
	//				["Approvals"] = approvals,
	//				["Workflow"] = workflow,
	//				["CurrentLevel"] = approvals.Any() ? approvals.Max(a => a.ApprovalLevel.Level) : 0,
	//				["TotalLevels"] = workflow?.Levels.Count() ?? 0,
	//				["IsFullyApproved"] = transaction.State == TransactionState.Approved,
	//				["IsRejected"] = transaction.State == TransactionState.Rejected,
	//				["IsPending"] = transaction.State == TransactionState.PendingApproval ||
	//							   transaction.State == TransactionState.PendingMultipleApproval ||
	//							   transaction.State == TransactionState.PartiallyApproved
	//			};

	//			return result;
	//		}

	//		// Log an action
	//		private async Task LogActionAsync(Guid transactionId, string action, string userId, string ipAddress, string detail)
	//		{
	//			var log = new TransactionLog
	//			{
	//				Id = Guid.NewGuid(),
	//				TransactionId = transactionId,
	//				Action = action,
	//				UserId = userId,
	//				Timestamp = DateTime.Now,
	//				IpAddress = ipAddress,
	//				Detail = detail
	//			};

	//			_context.TransactionLogs.Add(log);
	//		}

	//		// Send notifications to approvers
	//		private async Task SendApprovalNotificationsAsync(Transaction transaction, ApprovalLevel level)
	//		{
	//			// Get users with the required role
	//			var role = await _context.Roles.FindAsync(level.RoleId);
	//			if (role == null)
	//			{
	//				return;
	//			}

	//			// Get users with the role, excluding the transaction creator
	//			var userIds = await _context.UserRoles
	//				.Where(ur => ur.RoleId == role.Id)
	//				.Join(_context.Users.Where(u => u.Id != transaction.CreatedById),
	//					ur => ur.UserId,
	//					u => u.Id,
	//					(ur, u) => u.Id)
	//				.ToListAsync();

	//			// Send notification to each user
	//			foreach (var userId in userIds)
	//			{
	//				await _notificationService.CreateNotificationAsync(
	//					userId,
	//					"Transaction Pending Approval",
	//					$"A transaction requires your approval: {transaction.Description} for {transaction.Amount:C}",
	//					"PendingApproval",
	//					$"/Transaction/Details/{transaction.Id}",
	//					transaction.Id.ToString(),
	//					"Transaction"
	//				);
	//			}
	//		}
}
