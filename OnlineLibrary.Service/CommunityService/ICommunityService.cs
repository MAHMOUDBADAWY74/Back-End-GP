using OnlineLibrary.Service.CommunityService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService
{
    public interface ICommunityService
    {
        Task<CommunityDto> CreateCommunityAsync(CreateCommunityDto dto, string adminId);
        Task<IEnumerable<CommunityDto>> GetAllCommunitiesAsync();
        Task<CommunityDto> GetCommunityByIdAsync(long id);
        Task JoinCommunityAsync(long communityId, string userId);
        Task LeaveCommunityAsync(long communityId, string userId);





        Task<CommunityPostDto> CreatePostAsync(CreatePostDto dto, string userId);
        Task<IEnumerable<CommunityPostDto>> GetCommunityPostsAsync(long communityId, string currentUserId);
        Task LikePostAsync(long postId, string userId);
        Task UnlikePostAsync(long postId, string userId);


        Task<PostCommentDto> AddCommentAsync(CreateCommentDto dto, string userId);
        Task<IEnumerable<PostCommentDto>> GetPostCommentsAsync(long postId);


        Task SharePostAsync(long postId, string userId, long? sharedWithCommunityId);


        Task AssignModeratorAsync(AssignModeratorDto dto, string adminId);
        Task RemoveModeratorAsync(long communityId, string userId, string adminId);


        Task DeletePostAsync(long postId, string requesterId);
        Task DeleteCommentAsync(long commentId, string requesterId);
        Task BanUserAsync(long communityId, string userId, string requesterId);
        Task<bool> UnbanUserAsync(long communityId, string moderatorId, string userIdToUnban);
    }
}
