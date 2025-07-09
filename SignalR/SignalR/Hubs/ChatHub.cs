using Microsoft.AspNetCore.SignalR;

namespace SignalR.Hubs
{
    public class ChatHub : Hub
    {
        public static int clientscount = 0;

        public async Task SendMessageABC(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task ShowClients()
        {
            await Clients.All.SendAsync("ShowClients", clientscount);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Interlocked.Decrement(ref clientscount);
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            // Ví dụ: Xóa client khỏi group
            await Groups.RemoveFromGroupAsync(connectionId, "ChatGroup");

            // Ghi log nếu có lỗi
            if (exception != null)
            {
                Console.WriteLine($"Connection {connectionId} disconnected with error: {exception.Message}");
            }
            else
            {
                Console.WriteLine($"Connection {connectionId} disconnected gracefully.");
            }
            await ShowClients();
            await base.OnDisconnectedAsync(exception);
        }
        public override async Task OnConnectedAsync()
        {
            Interlocked.Increment(ref clientscount);
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            // Ví dụ: Gửi thông báo đến chính client vừa kết nối
            //await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome {userName}!");

            // Ví dụ: Gửi thông báo đến tất cả client khác

            // Gọi base method
            await ShowClients();
            await base.OnConnectedAsync();
        }
    }
}
