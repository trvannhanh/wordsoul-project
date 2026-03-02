

namespace WordSoul.Application.Interfaces.Services
{
    public interface IRealtimeNotificationService
    {
        Task SendNotificationAsync(int userId, object notification);
    }
}
