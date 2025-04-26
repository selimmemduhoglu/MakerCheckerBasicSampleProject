using MakerCheckerBasicSampleProject.Models.Entities;

namespace MakerCheckerBasicSampleProject.Models.ViewModels;

// Notification View Model
public class NotificationViewModel
{
	public List<Notification> Notifications { get; set; }
	public int UnreadCount { get; set; }
	public PageInfo PageInfo { get; set; }
}
