using TherapyCenter.Entities;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;

        public ChatService(IChatRepository chatRepo)
        {
            _chatRepo = chatRepo;
        }

        public async Task<ChatMessage> SaveMessageAsync(int senderId, string message)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            return await _chatRepo.CreateAsync(chatMessage);
        }

        public async Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            return await _chatRepo.GetRecentAsync(count);
        }
    }
}