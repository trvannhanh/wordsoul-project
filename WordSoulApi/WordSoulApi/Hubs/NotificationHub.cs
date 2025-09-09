using Microsoft.AspNetCore.SignalR;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SenNotification(Notification notification)
        {
            //Gửi đến user cụ thể dựa trên UserId
            await Clients.User(notification.UserId.ToString()).SendAsync("ReceiveNotification", notification);
        }
    }
}
