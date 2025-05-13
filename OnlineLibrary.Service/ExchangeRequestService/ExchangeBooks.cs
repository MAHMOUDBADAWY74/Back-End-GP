using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Repositories;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService
{
   public class ExchangeBooks : IExchangeBooks
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExchangeBooks(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<bool> AcceptExchangeRequestAsync(string acceptingUserId, AcceptExchangeRequestDto acceptDto)
        {
            var request = await _unitOfWork.Repository<ExchangeBookRequestx>().GetByIdAsync(acceptDto.RequestId);
            if (request == null || request.IsAccepted == true)
            {
                return false;
            }

            request.IsAccepted = true;
            request.ReceiverUserId = acceptingUserId;
            var receiver = await _userManager.FindByIdAsync(acceptingUserId);
            request.ReceiverName = receiver != null ? $"{receiver.firstName} {receiver.LastName}" : null;

            _unitOfWork.Repository<ExchangeBookRequestx>().Update(request);
            await _unitOfWork.CountAsync();
            return true;
        }

        public async Task CreateExchangeRequestAsync(string userId, CreateExchangeRequestDto requestDto)
        {
            var exchangeRequest = _mapper.Map<ExchangeBookRequestx>(requestDto);
            exchangeRequest.SenderUserId = userId;
            var sender = await _userManager.FindByIdAsync(userId);
            exchangeRequest.SenderName = sender != null ? $"{sender.firstName} {sender.LastName}" : null;

            await _unitOfWork.Repository<ExchangeBookRequestx>().AddAsync(exchangeRequest);
            await _unitOfWork.CountAsync();
        }

        public async Task<List<ExchangeRequestDto>> GetAllPendingExchangeRequestsAsync(string currentUserId)
        {
            var requests = await _unitOfWork.Repository<ExchangeBookRequestx>()
                .GetAllAsync();

            return requests
               .Where(r => r.IsAccepted == false && r.SenderUserId != currentUserId)
                .Select(async r =>
                {
                    var dto = _mapper.Map<ExchangeRequestDto>(r);
                    var requestingUser = await _userManager.FindByIdAsync(r.SenderUserId);
                    dto.SenderUserName = requestingUser?.firstName + " " + requestingUser?.LastName;
                    return dto;
                }).Select(t => t.Result) 
                .ToList();
        }

        public async Task<ExchangeRequestDto> GetExchangeRequestByIdAsync(long requestId)
        {
            var request = await _unitOfWork.Repository<ExchangeBookRequestx>().GetByIdAsync(requestId);
            if (request == null)
            {
                return null;
            }
            var dto = _mapper.Map<ExchangeRequestDto>(request);
            var requestingUser = await _userManager.FindByIdAsync(request.SenderUserId);
            dto.SenderUserName = requestingUser?.firstName + " " + requestingUser?.LastName;
            if (request.ReceiverUserId != null)
            {
                var acceptingUser = await _userManager.FindByIdAsync(request.ReceiverUserId);
                dto.ReceiverUserName = acceptingUser?.firstName + " " + acceptingUser?.LastName;
            }
            return dto;
        }
    }
}
