using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specifications;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IUnitOfWork unitOfWork, IHubContext<ChatHub> chatHub, ILogger<ChatController> logger)
        {
            _unitOfWork = unitOfWork;
            _chatHub = chatHub;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
        {
            var senderId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(messageDto.ReceiverId))
                return BadRequest("Sender or Receiver ID is missing.");

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Message = messageDto.Message
            };

            await _unitOfWork.Repository<ChatMessage>().AddAsync(message);
            await _unitOfWork.CountAsync();

            await _chatHub.Clients.User(messageDto.ReceiverId).SendAsync("ReceiveMessage", senderId, message.Message);

            return Ok(new { MessageId = message.Id });
        }

        [HttpGet("messages/{receiverId}")]
        public async Task<IActionResult> GetMessages(string receiverId)
        {
            var senderId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
                return BadRequest("Sender or Receiver ID is missing.");

            var messages = await _unitOfWork.Repository<ChatMessage>().GetAllWithSpecAsync(
                new ChatMessageSpecification(senderId, receiverId));

            var messagesToUpdate = messages.Where(m => m.ReceiverId == senderId && !m.IsRead).ToList();
            if (messagesToUpdate.Any())
            {
                _logger.LogInformation($"Found {messagesToUpdate.Count} messages to mark as read for user {senderId}");
                foreach (var message in messagesToUpdate)
                {
                    message.IsRead = true;
                    _unitOfWork.Repository<ChatMessage>().Update(message);
                    _logger.LogInformation($"Marked message {message.Id} as read");
                }
            }
            else
            {
                _logger.LogInformation("No messages to mark as read");
            }

            var updatedCount = await _unitOfWork.CountAsync();
            _logger.LogInformation($"Updated {updatedCount} entities in the database");

            var messageDtos = messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = Guid.Parse(m.SenderId),
                ReceiverId = Guid.Parse(m.ReceiverId),
                Message = m.Message,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt,
                Sender = new ChatUserDto
                {
                    Id = Guid.Parse(m.Sender.Id),
                    FirstName = m.Sender.firstName,
                    LastName = m.Sender.LastName,
                    UserName = m.Sender.UserName,
                    Email = m.Sender.Email
                },
                Receiver = new ChatUserDto
                {
                    Id = Guid.Parse(m.Receiver.Id),
                    FirstName = m.Receiver.firstName,
                    LastName = m.Receiver.LastName,
                    UserName = m.Receiver.UserName,
                    Email = m.Receiver.Email
                }
            }).ToList();

            return Ok(messageDtos);
        }
    }
}