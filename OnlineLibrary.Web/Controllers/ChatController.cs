using Microsoft.AspNetCore.Authorization;
using OnlineLibrary.Web.Hubs.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specifications;
using OnlineLibrary.Service.UserProfileService.Dtos;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;

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
        private readonly OnlineLibraryIdentityDbContext _dbContext;

        public ChatController(
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> chatHub,
            ILogger<ChatController> logger,
            OnlineLibraryIdentityDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _chatHub = chatHub;
            _logger = logger;
            _dbContext = dbContext;
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var minutes = (int)(DateTime.UtcNow - createdAt).TotalMinutes;
            if (minutes < 60)
                return $"{minutes} min ago";
            var hours = (int)(DateTime.UtcNow - createdAt).TotalHours;
            if (hours < 24)
                return $"{hours} h ago";
            var days = (int)(DateTime.UtcNow - createdAt).TotalDays;
            return $"{days} d ago";
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
        {
            var senderId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(messageDto.ReceiverId))
                return BadRequest("Sender or Receiver ID is missing.");

            var sender = await _dbContext.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == senderId);
            if (sender == null)
                return BadRequest("Sender not found.");

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Message = messageDto.Message
            };

            await _unitOfWork.Repository<ChatMessage>().AddAsync(message);
            await _unitOfWork.CountAsync();

            var notification = new Notification
            {
                UserId = messageDto.ReceiverId,
                ActorUserId = senderId,
                ActorUserName = sender.UserName,
                ActorProfilePicture = sender.UserProfile?.ProfilePhoto ?? "default_profile.jpg",
                NotificationType = NotificationTypes.MessageReceived,
                Message = $"{sender.UserName} sent you a new message.",
                RelatedEntityId = message.Id,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Message = notification.Message,
                ActorUserId = notification.ActorUserId,
                ActorUserName = notification.ActorUserName,
                ActorProfilePicture = notification.ActorProfilePicture,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };

            await _chatHub.Clients.User(messageDto.ReceiverId)
                .SendAsync("ReceiveNotification", notificationDto);

            await _chatHub.Clients.User(messageDto.ReceiverId)
                .SendAsync("ReceiveMessage", senderId, message.Message);

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

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is missing.");

            var messages = await _unitOfWork.Repository<ChatMessage>().GetAllAsync();

            var lastMessages = messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
                .ToList();

            var otherUserIds = lastMessages
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToList();

            var users = await _dbContext.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .Include(u => u.UserProfile)
                .ToListAsync();

            var conversations = lastMessages.Select(m =>
            {
                var otherUserId = m.SenderId == userId ? m.ReceiverId : m.SenderId;
                var user = users.FirstOrDefault(u => u.Id == otherUserId);

                return new ConversationDto
                {
                    UserId = user?.Id,
                    UserName = user?.UserName,
                    ProfilePicture = user?.UserProfile?.ProfilePhoto ?? "default_profile.jpg",
                    LastMessage = m.Message
                };
            }).ToList();

            return Ok(conversations);
        }
    }
}
