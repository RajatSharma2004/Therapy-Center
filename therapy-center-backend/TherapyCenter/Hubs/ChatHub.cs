using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            if (role == "Admin" || role == "Receptionist")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
                await Clients.Group("Staff").SendAsync("UserJoined", $"{name} ({role}) joined");

                // Send recent message history to the newly connected user
                var recentMessages = await _chatService.GetRecentMessagesAsync(50);
                await Clients.Caller.SendAsync("MessageHistory", recentMessages);
            }
            else
            {
                // Disconnect non-staff users from chat
                Context.Abort();
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Admin" || role == "Receptionist")
            {
                await Clients.Group("Staff").SendAsync("UserLeft", $"{name} ({role}) left");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(message) || userIdClaim == null)
                return;

            var userId = int.Parse(userIdClaim);

            // Save to database
            var saved = await _chatService.SaveMessageAsync(userId, message);

            // Broadcast to all staff
            await Clients.Group("Staff").SendAsync("ReceiveMessage", new
            {
                id = saved.Id,
                senderId = userId,
                senderName = name,
                senderRole = role,
                message = saved.Message,
                sentAt = saved.SentAt
            });
        }
    }
}