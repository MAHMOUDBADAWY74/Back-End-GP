using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService
{
    public interface IExchangeBooks
    {
        Task CreateExchangeRequestAsync(string userId, CreateExchangeRequestDto requestDto);
        Task<List<ExchangeRequestDto>> GetAllPendingExchangeRequestsAsync(string currentUserId);
        Task<bool> AcceptExchangeRequestAsync(string acceptingUserId, AcceptExchangeRequestDto acceptDto);
        Task<ExchangeRequestDto> GetExchangeRequestByIdAsync(long requestId);
    }
}
