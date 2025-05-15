using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Service.ExchangeRequestService.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var requests = await _unitOfWork.Repository<ExchangeBookRequestx>().GetAllAsync();

            var tasks = requests
                .Where(r => r.IsAccepted == false) 
                .Select(async r =>
                {
                    var dto = _mapper.Map<ExchangeRequestDto>(r);
                    var sender = await _userManager.FindByIdAsync(r.SenderUserId);
                    var senderProfile = await _unitOfWork.Repository<UserProfile>().GetFirstOrDefaultAsync(p => p.UserId == r.SenderUserId);
                    dto.SenderUserId = r.SenderUserId;
                    dto.SenderUserName = sender?.UserName;
                    dto.SenderProfilePhoto = senderProfile?.ProfilePhoto;

                    if (r.ReceiverUserId != null)
                    {
                        var receiver = await _userManager.FindByIdAsync(r.ReceiverUserId);
                        var receiverProfile = await _unitOfWork.Repository<UserProfile>().GetFirstOrDefaultAsync(p => p.UserId == r.ReceiverUserId);
                        dto.ReceiverUserId = r.ReceiverUserId;
                        dto.ReceiverUserName = receiver?.UserName;
                        dto.ReceiverProfilePhoto = receiverProfile?.ProfilePhoto;
                    }
                    return dto;
                });

            var resultArray = await Task.WhenAll(tasks); 
            return resultArray.ToList(); 
        }

        public async Task<ExchangeRequestDto> GetExchangeRequestByIdAsync(long requestId)
        {
            var request = await _unitOfWork.Repository<ExchangeBookRequestx>().GetByIdAsync(requestId);
            if (request == null)
            {
                return null;
            }

            var dto = _mapper.Map<ExchangeRequestDto>(request);
            var sender = await _userManager.FindByIdAsync(request.SenderUserId);
            var senderProfile = await _unitOfWork.Repository<UserProfile>().GetFirstOrDefaultAsync(p => p.UserId == request.SenderUserId);
            dto.SenderUserId = request.SenderUserId;
            dto.SenderUserName = sender?.UserName;
            dto.SenderProfilePhoto = senderProfile?.ProfilePhoto;

            if (request.ReceiverUserId != null)
            {
                var receiver = await _userManager.FindByIdAsync(request.ReceiverUserId);
                var receiverProfile = await _unitOfWork.Repository<UserProfile>().GetFirstOrDefaultAsync(p => p.UserId == request.ReceiverUserId);
                dto.ReceiverUserId = request.ReceiverUserId;
                dto.ReceiverUserName = receiver?.UserName;
                dto.ReceiverProfilePhoto = receiverProfile?.ProfilePhoto;
            }

            return dto;
        }
    }
}