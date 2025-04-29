using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Service.CommunityService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService
{
    public class CommunityService : ICommunityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommunityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<PostCommentDto> AddCommentAsync(CreateCommentDto dto, string userId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(dto.PostId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            
            var isMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .Any(m => m.CommunityId == post.CommunityId && m.UserId == userId);

            if (!isMember)
            {
                throw new Exception("Only community members can comment.");
            }

            var comment = new PostComment
            {
                Content = dto.Content,
                UserId = userId,
                PostId = dto.PostId
            };

            await _unitOfWork.Repository<PostComment>().AddAsync(comment);
            await _unitOfWork.CountAsync();

            return _mapper.Map<PostCommentDto>(comment);
        }


        public async Task AssignModeratorAsync(AssignModeratorDto dto, string adminId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(dto.CommunityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            if (community.AdminId != adminId)
            {
                throw new Exception("Only the  admin can assign moderators.");
            }

            var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == dto.CommunityId && m.UserId == dto.UserId);

            if (member == null)
            {
                throw new Exception("User is not a member of this community.");
            }

            if ((bool)member.IsModerator)
            {
                throw new Exception("User is already a moderator.");
            }

            member.IsModerator = true;
            _unitOfWork.Repository<CommunityMember>().Update(member);
            await _unitOfWork.CountAsync();
        }


        public async Task BanUserAsync(long communityId, string userId, string requesterId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

          
            var isAdmin = community.AdminId == requesterId;
            var isModerator = false;

            if (!isAdmin)
            {
                var requesterMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == requesterId);

                isModerator = requesterMember?.IsModerator ?? false;
            }

            if (!isAdmin && !isModerator)
            {
                throw new Exception("Only admins and moderators can ban users.");
            }

            
            if (community.AdminId == userId)
            {
                throw new Exception("Cannot ban the community admin."); 
            }

            
            var targetMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == userId); //admin bs

            if (targetMember?.IsModerator == true && !isAdmin)
            {
                throw new Exception("Only the admin can ban other moderators.");
            }

          
            if (targetMember != null)
            {
                _unitOfWork.Repository<CommunityMember>().Delete(targetMember);
                await _unitOfWork.CountAsync();
            }
        }


        public async Task<CommunityDto> CreateCommunityAsync(CreateCommunityDto dto, string adminId)
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                throw new Exception("Admin user not found.");
            }

            var community = new Community
            {
                Name = dto.Name,
                Description = dto.Description,
                AdminId = adminId
            };

            await _unitOfWork.Repository<Community>().AddAsync(community);
            await _unitOfWork.CountAsync();

            //  auto
            var member = new CommunityMember
            {
                UserId = adminId,
                CommunityId = community.Id,
                IsModerator = true
            };

            await _unitOfWork.Repository<CommunityMember>().AddAsync(member);
            await _unitOfWork.CountAsync();

            return _mapper.Map<CommunityDto>(community);
        }

        public async Task<IEnumerable<CommunityDto>> GetAllCommunitiesAsync()
        {
            var communities = await _unitOfWork.Repository<Community>().GetAllAsync();
            return _mapper.Map<IEnumerable<CommunityDto>>(communities);
        }

        public async Task<CommunityDto> GetCommunityByIdAsync(long id)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(id);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            return _mapper.Map<CommunityDto>(community);
        }



        public async Task<CommunityPostDto> CreatePostAsync(CreatePostDto dto, string userId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(dto.CommunityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            
            var isMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .Any(m => m.CommunityId == dto.CommunityId && m.UserId == userId);

            if (!isMember)
            {
                throw new Exception("Only community members can create posts."); // after join 
            }

            var post = new CommunityPost
            {
                Content = dto.Content,
                
                UserId = userId,
                CommunityId = dto.CommunityId
            };

            await _unitOfWork.Repository<CommunityPost>().AddAsync(post);
            await _unitOfWork.CountAsync();

            return _mapper.Map<CommunityPostDto>(post);
        }

        public async Task<IEnumerable<CommunityPostDto>> GetCommunityPostsAsync(long communityId, string currentUserId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var posts = (await _unitOfWork.Repository<CommunityPost>().GetAllAsync())
                .Where(p => p.CommunityId == communityId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var postDtos = _mapper.Map<IEnumerable<CommunityPostDto>>(posts);

            
            foreach (var postDto in postDtos)
            {
                var isLiked = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                    .Any(pl => pl.PostId == postDto.Id && pl.UserId == currentUserId);
                postDto.IsLiked = isLiked;
            }

            return postDtos;
        }


        public async Task DeleteCommentAsync(long commentId, string requesterId)
        {
            var comment = await _unitOfWork.Repository<PostComment>().GetByIdAsync(commentId);
            if (comment == null)
            {
                throw new Exception("Comment not found.");
            }

            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync((long)comment.PostId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var community = await _unitOfWork.Repository<Community>().GetByIdAsync((long)post.CommunityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            
            var isCommentAuthor = comment.UserId == requesterId;
            var isPostAuthor = post.UserId == requesterId;
            var isAdmin = community.AdminId == requesterId;
            var isModerator = false;

            if (!isAdmin)
            {
                var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .FirstOrDefault(m => m.CommunityId == post.CommunityId && m.UserId == requesterId);

                isModerator = member?.IsModerator ?? false;
            }

            if (!isCommentAuthor && !isPostAuthor && !isAdmin && !isModerator)
            {
                throw new Exception("You don't have permission to delete this comment.");
            }

            _unitOfWork.Repository<PostComment>().Delete(comment);
            await _unitOfWork.CountAsync();
        }


        public async Task DeletePostAsync(long postId, string requesterId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var community = await _unitOfWork.Repository<Community>().GetByIdAsync((long)post.CommunityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            
            var isAuthor = post.UserId == requesterId;
            var isAdmin = community.AdminId == requesterId;
            var isModerator = false;

            if (!isAdmin)
            {
                var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .FirstOrDefault(m => m.CommunityId == post.CommunityId && m.UserId == requesterId);

                isModerator = member?.IsModerator ?? false;
            }

            if (!isAuthor && !isAdmin && !isModerator)
            {
                throw new Exception("You don't have permission to delete this post.");
            }

            _unitOfWork.Repository<CommunityPost>().Delete(post);
            await _unitOfWork.CountAsync();
        }







        public async Task<IEnumerable<PostCommentDto>> GetPostCommentsAsync(long postId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var comments = (await _unitOfWork.Repository<PostComment>().GetAllAsync())
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return _mapper.Map<IEnumerable<PostCommentDto>>(comments);
        }

        public async Task JoinCommunityAsync(long communityId, string userId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var existingMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == userId);

            if (existingMember != null)
            {
                throw new Exception("User is already a member of this community.");
            }

            var member = new CommunityMember
            {
                UserId = userId,
                CommunityId = communityId,
                IsModerator = false
            };

            await _unitOfWork.Repository<CommunityMember>().AddAsync(member);
            await _unitOfWork.CountAsync();
        }

        public async Task LeaveCommunityAsync(long communityId, string userId)
        {
            var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == userId);

            if (member == null)
            {
                throw new Exception("User is not a member of this community.");
            }

           
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community.AdminId == userId)
            {
                throw new Exception("Admin cannot leave the community. Transfer first.");
            }

            _unitOfWork.Repository<CommunityMember>().Delete(member);
            await _unitOfWork.CountAsync();
        }


        public async Task LikePostAsync(long postId, string userId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var existingLike = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .FirstOrDefault(pl => pl.PostId == postId && pl.UserId == userId);

            if (existingLike != null)
            {
                throw new Exception("User has already liked this post.");
            }

            var like = new PostLike
            {
                UserId = userId,
                PostId = postId
            };

            await _unitOfWork.Repository<PostLike>().AddAsync(like);
            await _unitOfWork.CountAsync();
        }



        public async Task RemoveModeratorAsync(long communityId, string userId, string adminId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            if (community.AdminId != adminId)
            {
                throw new Exception("Only the community admin can remove moderators.");
            }

            var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == userId);

            if (member == null)
            {
                throw new Exception("User is not a member of this community.");
            }

            if ((bool)!member.IsModerator)
            {
                throw new Exception("User is not a moderator.");
            }

            member.IsModerator = false;
            _unitOfWork.Repository<CommunityMember>().Update(member);
            await _unitOfWork.CountAsync();
        }


        public async Task SharePostAsync(long postId, string userId, long? sharedWithCommunityId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (sharedWithCommunityId.HasValue)
            {
                var community = await _unitOfWork.Repository<Community>().GetByIdAsync(sharedWithCommunityId.Value);
                if (community == null)
                {
                    throw new Exception("Community not found.");
                }

                var isMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .Any(m => m.CommunityId == sharedWithCommunityId.Value && m.UserId == userId);

                if (!isMember)
                {
                    throw new Exception("You must be a member of the community to share posts there.");
                }
            }

            var share = new PostShare
            {
                UserId = userId,
                PostId = postId,
                SharedWithCommunityId = sharedWithCommunityId
            };

            await _unitOfWork.Repository<PostShare>().AddAsync(share);
            await _unitOfWork.CountAsync();
        }



        public async Task<bool> UnbanUserAsync(long communityId, string moderatorId, string userIdToUnban)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

           
            var isAdmin = community.AdminId == moderatorId;
            var isModerator = false;

            if (!isAdmin)
            {
                var requesterMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == moderatorId);

                isModerator = requesterMember?.IsModerator ?? false;
            }

            if (!isAdmin && !isModerator)
            {
                throw new Exception("Only admins and moderators can unban users.");
            }

           
            var isMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .Any(m => m.CommunityId == communityId && m.UserId == userIdToUnban);

            if (isMember)
            {
                return false; 
            }

            var member = new CommunityMember
            {
                UserId = userIdToUnban,
                CommunityId = communityId,
                IsModerator = false
            };

            await _unitOfWork.Repository<CommunityMember>().AddAsync(member);
            await _unitOfWork.CountAsync();

            return true;
        }


        public async Task UnlikePostAsync(long postId, string userId)
        {
            var like = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .FirstOrDefault(pl => pl.PostId == postId && pl.UserId == userId);

            if (like == null)
            {
                throw new Exception("User has not liked this post.");
            }

            _unitOfWork.Repository<PostLike>().Delete(like);
            await _unitOfWork.CountAsync();
        }

    }
}
