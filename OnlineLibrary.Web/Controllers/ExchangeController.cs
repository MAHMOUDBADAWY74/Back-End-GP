using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.ExchangeRequestService;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using System.Security.Claims;

namespace OnlineLibrary.Web.Controllers
{
   
    public class ExchangeController : BaseController
    {

        private readonly IExchangeBooks _exchangeBooks;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExchangeController
            (IExchangeBooks exchangeBooks ,
             UserManager<ApplicationUser> userManager)
            
        {
            _exchangeBooks = exchangeBooks;
            _userManager = userManager;

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
