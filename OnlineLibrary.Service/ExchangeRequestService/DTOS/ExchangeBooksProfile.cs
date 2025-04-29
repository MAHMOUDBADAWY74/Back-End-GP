using AutoMapper;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ExchangeRequestService.DTOS
{
    public class ExchangeBooksProfile :Profile 

    {
        public ExchangeBooksProfile()
        {

            CreateMap<ExchangeBookRequestx, ExchangeRequestDto>().ReverseMap();
            CreateMap<ExchangeBookRequestx, AcceptExchangeRequestDto>().ReverseMap();
            CreateMap<ExchangeBookRequestx, CreateExchangeRequestDto>().ReverseMap();

        }
    }
}
