using MakerCheckerBasicSampleProject.Configuration.Ef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MakerCheckerBasicSampleProject.SignalR;

[Authorize]
public class NotificationHub : Hub
{
	private readonly ApplicationDbContext _context;

	public NotificationHub(ApplicationDbContext context)
	{
		_context = context;
	}

	// Client connection
	public override async Task OnConnectedAsync()
	{
		var userId = Context.User.Identity.Name;
		await Groups.AddToGroupAsync(Context.ConnectionId, userId);
		await base.OnConnectedAsync();
	}

	// Client disconnection
	public override async Task OnDisconnectedAsync(Exception exception)
	{
		var userId = Context.User.Identity.Name;
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
		await base.OnDisconnectedAsync(exception);
	}

	// Method to mark notification as read
	public async Task MarkAsRead(string notificationId)
	{
		var userId = Context.User.Identity.Name;
		var notification = await _context.Notifications
			.FirstOrDefaultAsync(n => n.Id.ToString() == notificationId && n.UserId == userId);

		if (notification != null)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.Now;
			await _context.SaveChangesAsync();

			// Send updated unread count
			var unreadCount = await _context.Notifications
				.CountAsync(n => n.UserId == userId && !n.IsRead);
			await Clients.Caller.SendAsync("ReceiveUnreadCount", unreadCount);
		}
	}

	// Method to get unread notifications
	public async Task GetUnreadNotifications()
	{
		var userId = Context.User.Identity.Name;
		var notifications = await _context.Notifications
			.Where(n => n.UserId == userId && !n.IsRead)
			.OrderByDescending(n => n.CreatedAt)
			.Take(10)
			.ToListAsync();

		var unreadCount = await _context.Notifications
			.CountAsync(n => n.UserId == userId && !n.IsRead);

		await Clients.Caller.SendAsync("ReceiveUnreadNotifications", notifications);
		await Clients.Caller.SendAsync("ReceiveUnreadCount", unreadCount);
	}
}