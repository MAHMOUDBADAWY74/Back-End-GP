using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Service.AdminService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService
{
    public interface ICommunityService
    {
        Task<IEnumerable<CommunityMember>> GetCommunityMembersAsync(long communityId);
        Task<CommunityDto> CreateCommunityAsync(CreateCommunityDto dto, string adminId);
        Task<IEnumerable<CommunityDto>> GetAllCommunitiesAsync(string userId = null, bool isAdmin = false);
        Task<IEnumerable<CommunitySummaryDto>> GetAllCommunitiesSummaryAsync();
        Task<CommunityDto> GetCommunityByIdAsync(long id);
        Task JoinCommunityAsync(long communityId, string userId);
        Task LeaveCommunityAsync(long communityId, string userId);
        Task<CommunityPostDto> CreatePostAsync(CreatePostDto dto, string userId);
        Task<IEnumerable<CommunityPostDto>> GetCommunityPostsAsync(long communityId, string currentUserId);
        Task<IEnumerable<CommunityPostDto>> GetUserPostsAsync(string userId);
        Task<IEnumerable<CommunityPostDto>> GetAllCommunityPostsAsync(int pageNumber = 1, int pageSize = 20, string currentUserId = null);
        Task<IEnumerable<CommunityPostSummaryDto>> GetAllCommunityPostsSummaryAsync(int pageNumber = 1, int pageSize = 20);
        Task<IEnumerable<CommunityPostsCountDto>> GetCommunityPostsCountAsync();
        Task<MonthlyVisitsDto> GetMonthlyVisitsAsync(int year);
        Task<PostCommentDto> AddCommentAsync(CreateCommentDto dto, string userId);
        Task<IEnumerable<PostCommentDto>> GetPostCommentsAsync(long postId);
        Task LikePostAsync(long postId, string userId);
        Task UnlikePostAsync(long postId, string userId);
        Task SharePostAsync(long postId, string userId, long? sharedWithCommunityId);
        Task AssignModeratorAsync(AssignModeratorDto dto, string adminId);
        Task RemoveModeratorAsync(long communityId, string userId, string adminId);
        Task DeletePostAsync(long postId, string requesterId);
        Task DeleteCommentAsync(long commentId, string requesterId);
        Task BanUserAsync(long communityId, string userId, string requesterId);
        Task<bool> UnbanUserAsync(long communityId, string moderatorId, string userIdToUnban);
    }
}