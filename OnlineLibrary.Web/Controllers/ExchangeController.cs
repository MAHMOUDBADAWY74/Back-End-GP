using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.ExchangeRequestService;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Web.Hubs.Dtos;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnlineLibrary.Service.BookService.Dtos;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : BaseController
    {
        private readonly IExchangeBooks _exchangeBooks;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ExchangeController(
            IExchangeBooks exchangeBooks,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> notificationHub)
        {
            _exchangeBooks = exchangeBooks;
            _userManager = userManager;
            _notificationHub = notificationHub;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private async Task<(string Username, string ProfilePicture)> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var profilePicture = user?.UserProfile?.ProfilePhoto ?? "default_profile.jpg";
            var username = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            return (username, profilePicture);
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

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateExchangeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var (username, profilePicture) = await GetUserDetails(userId);

            await _exchangeBooks.CreateExchangeRequestAsync(userId, requestDto);

            var requests = await _exchangeBooks.GetAllPendingExchangeRequestsAsync(userId);
            var lastRequest = requests.LastOrDefault();
            if (lastRequest == null || string.IsNullOrEmpty(lastRequest.ReceiverUserId))
                return Ok("Exchange request created successfully.");

            var notification = new NotificationDto
            {
                NotificationType = "ExchangeRequestSent",
                Message = $"{username} sent you a book exchange request for '{requestDto.BookTitle}'.",
                ActorUserId = userId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                RelatedEntityId = lastRequest.Id,
                CreatedAt = DateTime.UtcNow,
                TimeAgo = GetTimeAgo(DateTime.UtcNow)
            };



            await _notificationHub.Clients.User(lastRequest.ReceiverUserId)
                .SendAsync("ReceiveNotification", notification);

            return Ok("Exchange request created successfully.");
        }



        [HttpGet]
        public async Task<ActionResult<List<ExchangeRequestDto>>> GetPendingRequests()
        {
            var userId = GetUserId();
            var requests = await _exchangeBooks.GetAllPendingExchangeRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptRequest([FromBody] AcceptExchangeRequestDto acceptDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var (username, profilePicture) = await GetUserDetails(userId);

            var result = await _exchangeBooks.AcceptExchangeRequestAsync(userId, acceptDto);
            if (result)
            {
                var request = await _exchangeBooks.GetExchangeRequestByIdAsync(acceptDto.RequestId);
                if (request != null)
                {
                    var notification = new NotificationDto
                    {
                        Id = 0,
                        NotificationType = "ExchangeRequestAccepted",
                        Message = $"{username} accepted your book exchange request for '{request.BookTitle}'.",
                        ActorUserId = userId,
                        ActorUserName = username,
                        ActorProfilePicture = profilePicture,
                        RelatedEntityId = request.Id,
                        CreatedAt = DateTime.UtcNow,
                        TimeAgo = GetTimeAgo(DateTime.UtcNow)
                    };

                    await _notificationHub.Clients.User(request.SenderUserId)
                        .SendAsync("ReceiveNotification", notification);
                }
                return Ok("Exchange request accepted.");
            }
            return BadRequest("Failed to accept.");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExchangeRequestDto>> GetRequestById(long id)
        {
            var request = await _exchangeBooks.GetExchangeRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return Ok(request);
        }
    }
}
