using AutoMapper;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService.DTOS
{
    public class ExchangeBooksProfile : Profile
    {
        public ExchangeBooksProfile()
        {
            CreateMap<ExchangeBookRequestx, ExchangeRequestDto>()
                .ForMember(dest => dest.SenderUserId, opt => opt.MapFrom(src => src.SenderUserId))
                .ForMember(dest => dest.ReceiverUserId, opt => opt.MapFrom(src => src.ReceiverUserId))
                .ForMember(dest => dest.SenderUserName, opt => opt.MapFrom(src => src.SenderName)) 
                .ForMember(dest => dest.ReceiverUserName, opt => opt.MapFrom(src => src.ReceiverName)) 
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ReverseMap();

            CreateMap<ExchangeBookRequestx, AcceptExchangeRequestDto>().ReverseMap();
            CreateMap<ExchangeBookRequestx, CreateExchangeRequestDto>().ReverseMap();
        }
    }
}