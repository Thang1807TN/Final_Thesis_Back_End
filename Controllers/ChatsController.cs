using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SecondHandMarketplaceAPI.DTOs.Chats;
using SecondHandMarketplaceAPI.Services.Interfaces;

namespace SecondHandMarketplaceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatsController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("conversations")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _chatService.CreateConversationAsync(userId, dto);

            if (result == null)
            {
                return BadRequest(new { message = "Could not create conversation." });
            }

            return Ok(result);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var conversations = await _chatService.GetMyConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId:int}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var messages = await _chatService.GetMessagesAsync(conversationId, userId);

            if (messages == null)
            {
                return NotFound(new { message = "Conversation not found." });
            }

            return Ok(messages);
        }

        [HttpPost("conversations/{conversationId:int}/messages")]
        public async Task<IActionResult> SendMessage(int conversationId, [FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var message = await _chatService.SendMessageAsync(conversationId, userId, dto);

            if (message == null)
            {
                return BadRequest(new { message = "Could not send message." });
            }

            return Ok(message);
        }
    }
}