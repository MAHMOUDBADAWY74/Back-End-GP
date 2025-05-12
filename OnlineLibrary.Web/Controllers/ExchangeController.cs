using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; 
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.ExchangeRequestService;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using OnlineLibrary.Web.Hubs; 
using System.Security.Claims;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateExchangeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            await _exchangeBooks.CreateExchangeRequestAsync(userId, requestDto);

           
            string message = $"A new exchange request has been created by {userId}! Check it out.";
            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to all users: {message}");

            return Ok("Exchange request created successfully.");
        }

        [HttpGet]
        public async Task<ActionResult<List<ExchangeRequestDto>>> GetPendingRequests()
        {
            var userId = GetUserId();
            var requests = await _exchangeBooks.GetAllPendingExchangeRequestsAsync(userId);
            return Ok(requests);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest([FromBody] AcceptExchangeRequestDto acceptDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var result = await _exchangeBooks.AcceptExchangeRequestAsync(userId, acceptDto);
            if (result)
            {
                return Ok("Exchange request accepted.");
            }
            return BadRequest("Failed to accept .");
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