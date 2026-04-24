using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.DTOs.Chats;
using SecondHandMarketplaceAPI.Hubs;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IEmailMockService _emailMockService;

        public ChatService(
            ApplicationDbContext context,
            IHubContext<ChatHub> hubContext,
            IEmailMockService emailMockService)
        {
            _context = context;
            _hubContext = hubContext;
            _emailMockService = emailMockService;
        }

        public async Task<ConversationResponseDto?> CreateConversationAsync(string userId, CreateConversationDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null || product.SellerId == userId)
            {
                return null;
            }

            var existingConversation = await _context.Conversations
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.ProductId == dto.ProductId &&
                    c.BuyerId == userId &&
                    c.SellerId == product.SellerId);

            if (existingConversation != null)
            {
                return MapConversation(existingConversation, userId);
            }

            var conversation = new Conversation
            {
                ProductId = product.Id,
                BuyerId = userId,
                SellerId = product.SellerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var created = await _context.Conversations
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .FirstAsync(c => c.Id == conversation.Id);

            return MapConversation(created, userId);
        }

        public async Task<IEnumerable<ConversationResponseDto>> GetMyConversationsAsync(string userId)
        {
            var conversations = await _context.Conversations
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == userId || c.SellerId == userId)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            return conversations.Select(c => MapConversation(c, userId));
        }

        public async Task<IEnumerable<MessageResponseDto>?> GetMessagesAsync(int conversationId, string userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
            {
                return null;
            }

            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var item in unreadMessages)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages.Select(MapMessage);
        }

        public async Task<MessageResponseDto?> SendMessageAsync(int conversationId, string userId, SendMessageDto dto)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(dto.Content) &&
                string.IsNullOrWhiteSpace(dto.AttachmentUrl))
            {
                return null;
            }

            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = dto.Content?.Trim() ?? string.Empty,
                AttachmentName = dto.AttachmentName,
                AttachmentUrl = dto.AttachmentUrl,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            conversation.LastMessageAt = message.SentAt;

            await _context.SaveChangesAsync();

            var created = await _context.Messages
                .Include(m => m.Sender)
                .FirstAsync(m => m.Id == message.Id);

            var result = MapMessage(created);

            await _hubContext.Clients.Group($"conversation-{conversationId}")
                .SendAsync("ReceiveMessage", result);

            var recipientUserId = conversation.BuyerId == userId
                ? conversation.SellerId
                : conversation.BuyerId;

            await _hubContext.Clients.Group($"user-{recipientUserId}")
                .SendAsync("ConversationUpdated", conversationId);

            var recipientEmail = conversation.BuyerId == userId
                ? conversation.Seller?.Email
                : conversation.Buyer?.Email;

            var recipientName = conversation.BuyerId == userId
                ? conversation.Seller?.FullName
                : conversation.Buyer?.FullName;

            await _emailMockService.SendAsync(
                recipientEmail ?? string.Empty,
                $"New message about {conversation.Product?.Title}",
                $"Hello {recipientName},\n\nYou have received a new message from {sender?.FullName} about product \"{conversation.Product?.Title}\".\n\nMessage:\n{dto.Content?.Trim()}\n\nThis is a mock notification from GreenMarket."
            );

            return result;
        }

        private static ConversationResponseDto MapConversation(Conversation conversation, string userId)
        {
            var lastMessage = conversation.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = conversation.Messages.Count(m => m.SenderId != userId && !m.IsRead);

            return new ConversationResponseDto
            {
                Id = conversation.Id,
                ProductId = conversation.ProductId,
                ProductTitle = conversation.Product?.Title ?? string.Empty,
                BuyerId = conversation.BuyerId,
                BuyerName = conversation.Buyer?.FullName ?? string.Empty,
                SellerId = conversation.SellerId,
                SellerName = conversation.Seller?.FullName ?? string.Empty,
                CreatedAt = conversation.CreatedAt,
                LastMessageAt = conversation.LastMessageAt,
                LastMessagePreview = lastMessage?.Content ?? (lastMessage?.AttachmentName ?? string.Empty),
                UnreadCount = unreadCount
            };
        }

        private static MessageResponseDto MapMessage(Message message)
        {
            return new MessageResponseDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName ?? string.Empty,
                Content = message.Content,
                AttachmentName = message.AttachmentName,
                AttachmentUrl = message.AttachmentUrl,
                SentAt = message.SentAt,
                IsRead = message.IsRead
            };
        }
    }
}