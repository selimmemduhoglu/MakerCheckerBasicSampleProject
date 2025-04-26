using MakerCheckerBasicSampleProject.Models;
using MakerCheckerBasicSampleProject.Models.ViewModels;
using MakerCheckerBasicSampleProject.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MakerCheckerBasicSampleProject.Configuration;

[Authorize]
public class NotificationController : Controller
{
	private readonly NotificationService _notificationService;
	private readonly UserManager<ApplicationUser> _userManager;

	public NotificationController(
		NotificationService notificationService,
		UserManager<ApplicationUser> userManager)
	{
		_notificationService = notificationService;
		_userManager = userManager;
	}

	// Get all notifications
	[HttpGet]
	public async Task<IActionResult> Index(int page = 1, int pageSize = 10, bool unreadOnly = false)
	{
		var userId = _userManager.GetUserId(User);

		// Get notifications
		var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly, page, pageSize);

		// Get total count for pagination
		//var totalCount = await _notificationService.GetNotificationsCountAsync(userId, unreadOnly);
		var totalCount = 10;

		// Get unread count
		var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

		// Create view model
		var viewModel = new NotificationViewModel
		{
			Notifications = notifications,
			UnreadCount = unreadCount,
			PageInfo = new PageInfo
			{
				CurrentPage = page,
				ItemsPerPage = pageSize,
				TotalItems = totalCount,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			}
		};

		// Set filter for view
		ViewBag.UnreadOnly = unreadOnly;

		return View(viewModel);
	}

	// Get notification partial for AJAX loading
	[HttpGet]
	public async Task<IActionResult> GetNotificationsPartial(bool unreadOnly = true)
	{
		var userId = _userManager.GetUserId(User);

		// Get latest notifications (limited to 10)
		var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly, 1, 10);

		// Get unread count
		var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

		ViewBag.UnreadCount = unreadCount;

		return PartialView("_NotificationsPartial", notifications);
	}

	// Mark notification as read
	[HttpPost]
	public async Task<IActionResult> MarkAsRead(Guid id)
	{
		var userId = _userManager.GetUserId(User);

		await _notificationService.MarkAsReadAsync(id, userId);

		// Return JSON response for AJAX
		return Json(new { success = true });
	}

	// Mark all notifications as read
	[HttpPost]
	public async Task<IActionResult> MarkAllAsRead()
	{
		var userId = _userManager.GetUserId(User);

		await _notificationService.MarkAllAsReadAsync(userId);

		// Return JSON response for AJAX
		return Json(new { success = true });
	}

	// Delete notification
	[HttpPost]
	public async Task<IActionResult> Delete(Guid id)
	{
		var userId = _userManager.GetUserId(User);

		await _notificationService.DeleteNotificationAsync(id, userId);

		// Return JSON response for AJAX
		return Json(new { success = true });
	}

	// Get unread count for AJAX refresh
	[HttpGet]
	public async Task<IActionResult> GetUnreadCount()
	{
		var userId = _userManager.GetUserId(User);

		var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

		return Json(new { count = unreadCount });
	}
}
