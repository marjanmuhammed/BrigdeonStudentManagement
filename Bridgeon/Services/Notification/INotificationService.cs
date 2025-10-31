// Services/Interfaces/INotificationService.cs
using Bridgeon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bridgeon.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId);
        Task<Notification> GetNotificationByIdAsync(int id);
    }
}