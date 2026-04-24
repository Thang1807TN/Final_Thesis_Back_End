using SecondHandMarketplaceAPI.DTOs.Chats;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IChatService
    {
        Task<ConversationResponseDto?> CreateConversationAsync(string userId, CreateConversationDto dto);
        Task<IEnumerable<ConversationResponseDto>> GetMyConversationsAsync(string userId);
        Task<IEnumerable<MessageResponseDto>?> GetMessagesAsync(int conversationId, string userId);
        Task<MessageResponseDto?> SendMessageAsync(int conversationId, string userId, SendMessageDto dto);
    }
}