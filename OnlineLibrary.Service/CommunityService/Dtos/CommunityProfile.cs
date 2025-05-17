using AutoMapper;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.AdminService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CommunityProfile : Profile
    {
        public CommunityProfile()
        {
            CreateMap<Community, CommunityDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.Admin != null ? $"{src.Admin.firstName} {src.Admin.LastName}" : "Unknown"))
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsMember, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.MapFrom(src => src.Posts != null ? src.Posts.Count : 0));

            CreateMap<CommunityMember, CommunityMemberDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.firstName} {src.User.LastName}" : "Unknown"));

            CreateMap<CommunityPost, CommunityPostDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.firstName} {src.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.CommunityName, opt => opt.MapFrom(src => src.Community != null ? src.Community.Name : "Unknown"))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.Likes != null ? src.Likes.Count : 0))
                .ForMember(dest => dest.UnlikeCount, opt => opt.Ignore())
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))
                .ForMember(dest => dest.ShareCount, opt => opt.MapFrom(src => src.Shares != null ? src.Shares.Count : 0))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
                .ForMember(dest => dest.IsUnliked, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? src.User.UserProfile.ProfilePhoto : null));

            CreateMap<CommunityPost, CommunityPostSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.CommunityId, opt => opt.MapFrom(src => src.CommunityId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            CreateMap<PostComment, PostCommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.firstName} {src.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.User != null && src.User.UserProfile != null ? src.User.UserProfile.ProfilePhoto : null));

            CreateMap<PostShare, PostShareDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.firstName} {src.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.SharedWithCommunityName, opt => opt.MapFrom(src => src.SharedWithCommunity != null ? src.SharedWithCommunity.Name : "Unknown"));

            CreateMap<CreateCommunityDto, Community>();
            CreateMap<CreatePostDto, CommunityPost>();
            CreateMap<CreateCommentDto, PostComment>();
            CreateMap<PostUnlike, PostUnlikeDto>().ReverseMap();
        }
    }
}

