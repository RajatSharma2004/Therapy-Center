using TherapyCenter.Entities;

namespace TherapyCenter.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessage> SaveMessageAsync(int senderId, string message);
        Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int count = 50);
    }
}