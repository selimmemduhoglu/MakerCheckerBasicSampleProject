using MakerCheckerBasicSampleProject.Configuration.Ef;
using MakerCheckerBasicSampleProject.Models.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static MakerCheckerBasicSampleProject.Models.ApplicationUser;

namespace MakerCheckerBasicSampleProject.SignalR;

public class NotificationService
{
	private readonly ApplicationDbContext _context;
	private readonly IHubContext<NotificationHub> _hubContext;

	public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
	{
		_context = context;
		_hubContext = hubContext;
	}

	// Create and send notification to a specific user
	public async Task CreateNotificationAsync(string userId, string title, string message, string type,
		string url = null, string relatedEntityId = null, string relatedEntityType = null)
	{
		var notification = new Notification
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Title = title,
			Message = message,
			Type = type,
			Url = url,
			RelatedEntityId = relatedEntityId,
			RelatedEntityType = relatedEntityType,
			CreatedAt = DateTime.Now,
			IsRead = false
		};

		_context.Notifications.Add(notification);
		await _context.SaveChangesAsync();

		// Send real-time notification
		await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);

		// Update unread count
		var unreadCount = await _context.Notifications
			.CountAsync(n => n.UserId == userId && !n.IsRead);
		await _hubContext.Clients.Group(userId).SendAsync("ReceiveUnreadCount", unreadCount);
	}

	// Create and send notification to users with a specific role
	public async Task CreateNotificationForRoleAsync(string roleName, string title, string message, string type,
		string url = null, string relatedEntityId = null, string relatedEntityType = null)
	{
		// Get all users with the specified role
		var userIds = await _context.UserRoles
			.Where(ur => ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == roleName).Id)
			.Join(_context.Users,
				ur => ur.UserId,
				u => u.Id,
				(ur, u) => u.Id)
			.ToListAsync();

		// Create notification for each user
		foreach (var userId in userIds)
		{
			await CreateNotificationAsync(userId, title, message, type, url, relatedEntityId, relatedEntityType);
		}
	}

	// Get notifications for a user
	public async Task<List<Notification>> GetNotificationsAsync(string userId, bool unreadOnly = false, int page = 1, int pageSize = 10)
	{
		var query = _context.Notifications
			.Where(n => n.UserId == userId);

		if (unreadOnly)
		{
			query = query.Where(n => !n.IsRead);
		}

		return await query
			.OrderByDescending(n => n.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
	}

	// Get total count of notifications
	public async Task<int> GetNotificationsCountAsync(string userId, bool unreadOnly = false)
	{
		var query = _context.Notifications
			.Where(n => n.UserId == userId);

		if (unreadOnly)
		{
			query = query.Where(n => !n.IsRead);
		}

		return await query.CountAsync();
	}

	// Get unread notification count for a user
	public async Task<int> GetUnreadCountAsync(string userId)
	{
		return await _context.Notifications
			.CountAsync(n => n.UserId == userId && !n.IsRead);
	}

	// Mark notification as read
	public async Task MarkAsReadAsync(Guid notificationId, string userId)
	{
		var notification = await _context.Notifications
			.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

		if (notification != null)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.Now;
			await _context.SaveChangesAsync();

			// Update unread count
			var unreadCount = await _context.Notifications
				.CountAsync(n => n.UserId == userId && !n.IsRead);
			await _hubContext.Clients.Group(userId).SendAsync("ReceiveUnreadCount", unreadCount);
		}
	}

	// Mark all notifications as read
	public async Task MarkAllAsReadAsync(string userId)
	{
		var notifications = await _context.Notifications
			.Where(n => n.UserId == userId && !n.IsRead)
			.ToListAsync();

		foreach (var notification in notifications)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.Now;
		}

		await _context.SaveChangesAsync();

		// Update unread count
		await _hubContext.Clients.Group(userId).SendAsync("ReceiveUnreadCount", 0);
	}

	// Delete a notification
	public async Task DeleteNotificationAsync(Guid notificationId, string userId)
	{
		var notification = await _context.Notifications
			.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

		if (notification != null)
		{
			_context.Notifications.Remove(notification);
			await _context.SaveChangesAsync();

			// Update unread count if it was unread
			if (!notification.IsRead)
			{
				var unreadCount = await _context.Notifications
					.CountAsync(n => n.UserId == userId && !n.IsRead);
				await _hubContext.Clients.Group(userId).SendAsync("ReceiveUnreadCount", unreadCount);
			}
		}
	}

	// Get notifications by related entity
	public async Task<List<Notification>> GetNotificationsByRelatedEntityAsync(
		string relatedEntityId, string relatedEntityType, int page = 1, int pageSize = 10)
	{
		return await _context.Notifications
			.Where(n => n.RelatedEntityId == relatedEntityId && n.RelatedEntityType == relatedEntityType)
			.OrderByDescending(n => n.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();
	}
}