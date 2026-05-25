namespace WordSoul.Application.Interfaces.Services
{
    public interface IFcmService
    {
        Task SendPushNotificationAsync(string fcmToken, string title, string body, string? actionUrl = null);
    }
}
