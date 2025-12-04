using Microsoft.AspNetCore.SignalR;
using WordSoul.Domain.Entities;

namespace WordSoul.Api.Hubs
{
    // Hub để quản lý thông báo thời gian thực
    public class NotificationHub : Hub
    {
        // Phương thức gửi thông báo đến client
        public async Task SenNotification(Notification notification)
        {
            //Gửi đến user cụ thể dựa trên UserId
            await Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
