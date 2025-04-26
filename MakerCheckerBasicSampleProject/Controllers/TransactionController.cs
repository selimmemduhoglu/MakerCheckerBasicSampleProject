//using MakerCheckerBasicSampleProject.Config.Configuration;
//using MakerCheckerBasicSampleProject.Configuration;
//using MakerCheckerBasicSampleProject.Configuration.Ef;
//using MakerCheckerBasicSampleProject.Models;
//using MakerCheckerBasicSampleProject.Models.Entities;
//using MakerCheckerBasicSampleProject.Models.ViewModels;
//using MakerCheckerBasicSampleProject.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;

//namespace MakerCheckerBasicSampleProject.Controllers;

//[Authorize]
//public class TransactionController : Controller
//{
//	private readonly TransactionService _transactionService;
//	private readonly UserManager<ApplicationUser> _userManager;
//	private readonly ApplicationDbContext _context;
//	private readonly IUserActivityService _userActivityService;

//	public TransactionController(
//		TransactionService transactionService,
//		UserManager<ApplicationUser> userManager,
//		ApplicationDbContext context,
//		IUserActivityService userActivityService)
//	{
//		_transactionService = transactionService;
//		_userManager = userManager;
//		_context = context;
//		_userActivityService = userActivityService;
//	}

//	[HttpGet]
//	public async Task<IActionResult> Dashboard()
//	{
//		var userId = _userManager.GetUserId(User);
//		var user = await _userManager.FindByIdAsync(userId);
//		var userRoles = await _userManager.GetRolesAsync(user);

//		// Get counts for various transaction types
//		var dashboardViewModel = new DashboardViewModel
//		{
//			PendingTransactionsCount = await GetTransactionCountAsync(3, userId), // Unverified Records
//			ApprovedTransactionsCount = await GetTransactionCountAsync(4, userId), // Verified Records
//			RejectedTransactionsCount = await GetTransactionCountAsync(5, userId), // Rejected Records
//			MyCreatedTransactionsCount = await GetTransactionCountAsync(6, userId), // My Created Transactions
//			UserRoles = userRoles.ToList(),
//			RecentTransactions = await _transactionService.GetTransactionsAsync(2, userId, 1, 5) // Recent transactions
//		};

//		// Add role-specific counts
//		if (userRoles.Contains("Checker") || userRoles.Contains("SupervisorChecker"))
//		{
//			dashboardViewModel.PendingMyApprovalCount = await GetTransactionCountAsync(9, userId); // Pending My Approval
//		}

//		// Get user activity
//		dashboardViewModel.RecentActivity = await _userActivityService.GetUserActivitiesAsync(userId, 1, 5);

//		return View(dashboardViewModel);
//	}



//	[HttpGet]
//	public async Task<IActionResult> Index(int filterId = 2, int page = 1, int pageSize = 10)
//	{
//		var userId = _userManager.GetUserId(User);
//		var user = await _userManager.FindByIdAsync(userId);
//		var userRoles = await _userManager.GetRolesAsync(user);

//		ViewBag.UserRoles = userRoles;
//		ViewBag.FilterId = filterId;
//		ViewBag.Page = page;
//		ViewBag.PageSize = pageSize;

//		// Get filters for dropdown
//		ViewBag.Filters = await GetFiltersAsync(userRoles);

//		// Get transactions based on filter
//		var transactions = await _transactionService.GetTransactionsAsync(filterId, userId, page, pageSize);

//		// Add PageInfo for pagination
//		var transactionCount = await GetTransactionCountAsync(filterId, userId);
//		ViewBag.PageInfo = new PageInfo
//		{
//			CurrentPage = page,
//			ItemsPerPage = pageSize,
//			TotalItems = transactionCount,
//			TotalPages = (int)Math.Ceiling((double)transactionCount / pageSize)
//		};

//		return View(transactions);
//	}

//	[HttpGet]
//	[Authorize(Policy = "Permission:Create_Transaction")]
//	public async Task<IActionResult> Create()
//	{
//		var userId = _userManager.GetUserId(User);

//		// Get available workflows
//		var workflows = await _transactionService.GetApprovalWorkflowsAsync();
//		ViewBag.Workflows = new SelectList(workflows, "Id", "Name");

//		return View(new CreateTransactionViewModel());
//	}

//	[HttpPost]
//	[ValidateAntiForgeryToken]
//	[Authorize(Policy = "Permission:Create_Transaction")]
//	public async Task<IActionResult> Create(CreateTransactionViewModel model)
//	{
//		if (ModelState.IsValid)
//		{
//			try
//			{
//				var userId = _userManager.GetUserId(User);

//				await _transactionService.CreateTransactionAsync(model.Description, model.Amount, userId, model.WorkflowId);

//				TempData["SuccessMessage"] = "Transaction created successfully.";
//				return RedirectToAction("Index");
//			}
//			catch (Exception ex)
//			{
//				ModelState.AddModelError("", ex.Message);
//			}
//		}

//		// Get available workflows for dropdown
//		var workflows = await _transactionService.GetApprovalWorkflowsAsync();
//		ViewBag.Workflows = new SelectList(workflows, "Id", "Name", model.WorkflowId);

//		return View(model);
//	}

//	[HttpGet]
//	public async Task<IActionResult> Details(Guid id)
//	{
//		var userId = _userManager.GetUserId(User);
//		var transaction = await _transactionService.GetTransactionAsync(id);

//		if (transaction == null)
//		{
//			TempData["ErrorMessage"] = "Transaction not found.";
//			return RedirectToAction("Index");
//		}

//		// Get transaction logs
//		ViewBag.Logs = await _transactionService.GetTransactionLogsAsync(id);

//		// Get transaction approvals
//		ViewBag.Approvals = await _transactionService.GetTransactionApprovalsAsync(id);

//		// Get approval status
//		ViewBag.ApprovalStatus = await _transactionService.GetTransactionApprovalStatusAsync(id);

//		// Check if user can approve/reject
//		var canApprove = await CanUserApproveTransactionAsync(transaction, userId);
//		ViewBag.CanApprove = canApprove;

//		return View(transaction);
//	}




//	[HttpPost]
//	[ValidateAntiForgeryToken]
//	[Authorize(Policy = "Permission:Approve_Transaction")]
//	public async Task<IActionResult> Approve(Guid id, string comment)
//	{
//		try
//		{
//			var userId = _userManager.GetUserId(User);

//			// Check if user can approve this transaction
//			var transaction = await _transactionService.GetTransactionAsync(id);
//			if (transaction == null)
//			{
//				TempData["ErrorMessage"] = "Transaction not found.";
//				return RedirectToAction("Index");
//			}

//			var canApprove = await CanUserApproveTransactionAsync(transaction, userId);
//			if (!canApprove)
//			{
//				TempData["ErrorMessage"] = "You don't have permission to approve this transaction.";
//				return RedirectToAction("Details", new { id });
//			}

//			await _transactionService.ApproveTransactionAsync(id, userId, comment);

//			TempData["SuccessMessage"] = "Transaction approved successfully.";
//			return RedirectToAction("Details", new { id });
//		}
//		catch (Exception ex)
//		{
//			TempData["ErrorMessage"] = ex.Message;
//			return RedirectToAction("Details", new { id });
//		}
//	}

//	[HttpPost]
//	[ValidateAntiForgeryToken]
//	[Authorize(Policy = "Permission:Reject_Transaction")]
//	public async Task<IActionResult> Reject(Guid id, string reason)
//	{
//		if (string.IsNullOrWhiteSpace(reason))
//		{
//			TempData["ErrorMessage"] = "Rejection reason is required.";
//			return RedirectToAction("Details", new { id });
//		}

//		try
//		{
//			var userId = _userManager.GetUserId(User);

//			// Check if user can reject this transaction
//			var transaction = await _transactionService.GetTransactionAsync(id);
//			if (transaction == null)
//			{
//				TempData["ErrorMessage"] = "Transaction not found.";
//				return RedirectToAction("Index");
//			}

//			var canApprove = await CanUserApproveTransactionAsync(transaction, userId);
//			if (!canApprove)
//			{
//				TempData["ErrorMessage"] = "You don't have permission to reject this transaction.";
//				return RedirectToAction("Details", new { id });
//			}

//			await _transactionService.RejectTransactionAsync(id, userId, reason);

//			TempData["SuccessMessage"] = "Transaction rejected successfully.";
//			return RedirectToAction("Details", new { id });
//		}
//		catch (Exception ex)
//		{
//			TempData["ErrorMessage"] = ex.Message;
//			return RedirectToAction("Details", new { id });
//		}
//	}


//	#region HELPER METHODS


//	// Helper method to get filters based on user roles
//	private async Task<SelectList> GetFiltersAsync(IList<string> userRoles)
//	{
//		var filters = new List<object>
//		{
//			new { Id = 2, Name = "All Records" },
//			new { Id = 1, Name = "Active Records" },
//			new { Id = 3, Name = "Unverified Records" },
//			new { Id = 4, Name = "Verified Records" },
//			new { Id = 5, Name = "Rejected Records" },
//			new { Id = 6, Name = "My Created Transactions" }
//		};

//		// Add role-specific filters
//		if (userRoles.Contains("Checker") || userRoles.Contains("SupervisorChecker"))
//		{
//			filters.Add(new { Id = 7, Name = "My Approved Transactions" });
//			filters.Add(new { Id = 8, Name = "My Rejected Transactions" });
//			filters.Add(new { Id = 9, Name = "Pending My Approval" });
//		}

//		return new SelectList(filters, "Id", "Name");
//	}

//	// Helper method to get transaction count for a specific filter
//	private async Task<int> GetTransactionCountAsync(int filterId, string userId)
//	{
//		// This will return total count without paging
//		var transactions = await _transactionService.GetTransactionsAsync(filterId, userId, 1, int.MaxValue);
//		return transactions.Count;
//	}

//	// Helper method to check if user can approve a transaction
//	private async Task<bool> CanUserApproveTransactionAsync(Transaction transaction, string userId)
//	{
//		// Cannot approve own transaction
//		if (transaction.CreatedById == userId)
//		{
//			return false;
//		}

//		// Cannot approve if not in pending state
//		if (transaction.State != TransactionState.PendingApproval &&
//			transaction.State != TransactionState.PendingMultipleApproval &&
//			transaction.State != TransactionState.PartiallyApproved)
//		{
//			return false;
//		}

//		// Get user roles
//		var user = await _userManager.FindByIdAsync(userId);
//		var userRoles = await _userManager.GetRolesAsync(user);

//		// Get approval workflow and current level
//		var approvalStatus = await _transactionService.GetTransactionApprovalStatusAsync(transaction.Id);
//		var workflow = approvalStatus["Workflow"] as ApprovalWorkflow;
//		var currentLevel = (int)approvalStatus["CurrentLevel"];

//		// For multi-level approval, check if user has the role for the next level
//		if (transaction.State == TransactionState.PendingMultipleApproval ||
//			transaction.State == TransactionState.PartiallyApproved)
//		{
//			// Find next level
//			var nextLevel = workflow?.Levels.FirstOrDefault(l => l.Level == currentLevel + 1);
//			if (nextLevel == null)
//			{
//				return false;
//			}

//			// Check if user has the role
//			var role = await _context.Roles.FindAsync(nextLevel.RoleId);
//			if (role == null || !userRoles.Contains(role.Name))
//			{
//				return false;
//			}
//		}
//		else
//		{
//			// For standard approval, check if user has Checker role
//			if (!userRoles.Contains("Checker") && !userRoles.Contains("SupervisorChecker"))
//			{
//				return false;
//			}
//		}

//		return true;
//	}


//	#endregion
//}