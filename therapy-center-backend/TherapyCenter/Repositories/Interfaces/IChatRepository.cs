using TherapyCenter.Entities;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatMessage> CreateAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetRecentAsync(int count = 50);
    }
}