using MakerCheckerBasicSampleProject.Services;
using MakerCheckerBasicSampleProject.StateFilter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MakerCheckerBasicSampleProject.Controllers;

public class TransactionController : Controller
{
	private readonly TransactionService _transactionService;

	public TransactionController(TransactionService transactionService)
	{
		_transactionService = transactionService;
	}

	// Dashboard - shows transactions based on filter
	public IActionResult Index(int filterId = 2) // Default to All Records
	{
		// For demo purposes, we'll use a hardcoded username
		string username = GetCurrentUsername();

		ViewBag.CurrentRole = _transactionService.GetUserRole(username);
		ViewBag.Username = username;
		ViewBag.FilterId = filterId;

		// Get filters for dropdown
		var filters = DefaultStateFilterFactory.GetStateFilter();
		ViewBag.Filters = new SelectList(
			filters.Keys.OrderBy(f => f.Order).Select(f => new { f.Id, f.Name }),
			"Id", "Name", filterId);

		// Get transactions based on filter
		var transactions = _transactionService.GetTransactions(filterId, username);

		return View(transactions);
	}

	// Create transaction form
	public IActionResult Create()
	{
		string username = GetCurrentUsername();
		if (_transactionService.GetUserRole(username) != "Maker")
		{
			TempData["ErrorMessage"] = "Only Maker role can create transactions";
			return RedirectToAction("Index");
		}

		return View();
	}

	// Create transaction submit
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Create(string description, decimal amount)
	{
		try
		{
			string username = GetCurrentUsername();
			string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

			_transactionService.CreateTransaction(description, amount, username, ipAddress);

			TempData["SuccessMessage"] = "Transaction created successfully";
			return RedirectToAction("Index");
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = ex.Message;
			return View();
		}
	}

	// View transaction details
	public IActionResult Details(Guid id)
	{
		var transactions = _transactionService.GetTransactions(2, GetCurrentUsername()); // Get all records
		var transaction = transactions.FirstOrDefault(t => t.Id == id);

		if (transaction == null)
		{
			TempData["ErrorMessage"] = "Transaction not found";
			return RedirectToAction("Index");
		}

		// Get logs for this transaction
		ViewBag.Logs = _transactionService.GetLogs(id);

		return View(transaction);
	}

	// Approve transaction
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Approve(Guid id)
	{
		try
		{
			string username = GetCurrentUsername();
			string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

			_transactionService.ApproveTransaction(id, username, ipAddress);

			TempData["SuccessMessage"] = "Transaction approved successfully";
			return RedirectToAction("Index");
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = ex.Message;
			return RedirectToAction("Details", new { id });
		}
	}

	// Reject transaction
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Reject(Guid id, string reason)
	{
		try
		{
			string username = GetCurrentUsername();
			string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

			_transactionService.RejectTransaction(id, username, ipAddress, reason);

			TempData["SuccessMessage"] = "Transaction rejected successfully";
			return RedirectToAction("Index");
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = ex.Message;
			return RedirectToAction("Details", new { id });
		}
	}

	// For demo purposes only
	[HttpPost]
	public IActionResult SwitchUser()
	{
		// Toggle between user1 and user2
		if (HttpContext.Session.GetString("CurrentUser") == "user1")
		{
			HttpContext.Session.SetString("CurrentUser", "user2");
		}
		else
		{
			HttpContext.Session.SetString("CurrentUser", "user1");
		}

		return RedirectToAction("Index");
	}

	// Helper method to get current user
	private string GetCurrentUsername()
	{
		// For demo purposes, we'll store current user in session
		if (string.IsNullOrEmpty(HttpContext.Session.GetString("CurrentUser")))
		{
			HttpContext.Session.SetString("CurrentUser", "user1");
		}

		return HttpContext.Session.GetString("CurrentUser");
	}
}
