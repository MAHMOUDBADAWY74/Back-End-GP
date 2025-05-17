using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Service.AdminService.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace OnlineLibrary.Service.CommunityService
{
    public class CommunityService : ICommunityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public CommunityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<IEnumerable<CommunityMember>> GetCommunityMembersAsync(long communityId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            return (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .Where(m => m.CommunityId == communityId)
                .ToList();
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
                AdminId = adminId,
                PostCount = 0
            };

            await _unitOfWork.Repository<Community>().AddAsync(community);
            await _unitOfWork.CountAsync();

            var member = new CommunityMember
            {
                UserId = adminId,
                CommunityId = community.Id,
                IsModerator = true,
                JoinedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CommunityMember>().AddAsync(member);
            await _unitOfWork.CountAsync();

            var communityDto = _mapper.Map<CommunityDto>(community);
            communityDto.MemberCount = 1;
            communityDto.IsMember = true;
            communityDto.AdminName = $"{admin.firstName} {admin.LastName}";
            return communityDto;
        }

        public async Task<IEnumerable<CommunityDto>> GetAllCommunitiesAsync(string userId = null, bool isAdmin = false)
        {
            var communities = await _unitOfWork.Repository<Community>().GetAllAsync();
            var communityMembers = await _unitOfWork.Repository<CommunityMember>().GetAllAsync();
            var communityPosts = await _unitOfWork.Repository<CommunityPost>().GetAllAsync();

            IEnumerable<Community> filteredCommunities = communities;

            var communityDtos = _mapper.Map<IEnumerable<CommunityDto>>(filteredCommunities);
            foreach (var dto in communityDtos)
            {
                dto.MemberCount = communityMembers.Count(m => m.CommunityId == dto.Id);
                dto.IsMember = !string.IsNullOrEmpty(userId) && communityMembers.Any(m => m.CommunityId == dto.Id && m.UserId == userId);
                var admin = await _userManager.FindByIdAsync(dto.AdminId);
                dto.AdminName = admin != null ? $"{admin.firstName} {admin.LastName}" : "Unknown";
                dto.PostCount = communityPosts.Count(p => p.CommunityId.HasValue && p.CommunityId.Value == dto.Id);
            }

            return communityDtos;
        }

        public async Task<CommunityDto> GetCommunityByIdAsync(long id)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(id);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var communityMembers = await _unitOfWork.Repository<CommunityMember>().GetAllAsync();
            var communityPosts = (await _unitOfWork.Repository<CommunityPost>().GetAllWithIncludeAsync(
                p => p.User,
                p => p.Community))
                .Where(p => p.CommunityId.HasValue && p.CommunityId.Value == id)
                .ToList();

            var communityDto = _mapper.Map<CommunityDto>(community);
            communityDto.MemberCount = communityMembers.Count(m => m.CommunityId == id);
            communityDto.PostCount = communityPosts.Count;
            communityDto.IsMember = false;
            var admin = await _userManager.FindByIdAsync(communityDto.AdminId);
            communityDto.AdminName = admin != null ? $"{admin.firstName} {admin.LastName}" : "Unknown";

            return communityDto;
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
                IsModerator = false,
                JoinedDate = DateTime.UtcNow
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
                throw new Exception("Only community members can create posts.");
            }

            var post = new CommunityPost
            {
                Content = dto.Content,
                UserId = userId,
                CommunityId = dto.CommunityId,
                CreatedAt = DateTime.UtcNow,
                CommentCount = 0
            };

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "post-images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                var filePath = Path.Combine(imagesFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                post.ImageUrl = $"/post-images/{uniqueFileName}";
            }

            await _unitOfWork.Repository<CommunityPost>().AddAsync(post);

            community.PostCount = (community.PostCount ?? 0) + 1;
            _unitOfWork.Repository<Community>().Update(community);

            await _unitOfWork.CountAsync();

            var postDto = _mapper.Map<CommunityPostDto>(post);
            postDto.UserName = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            postDto.CommunityName = community.Name;
            return postDto;
        }

        public async Task<IEnumerable<CommunityPostDto>> GetCommunityPostsAsync(long communityId, string currentUserId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var posts = (await _unitOfWork.Repository<CommunityPost>().GetAllWithIncludeAsync(
                p => p.User,
                p => p.Community))
                .Where(p => p.CommunityId.HasValue && p.CommunityId.Value == communityId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (!posts.Any())
            {
                return new List<CommunityPostDto>();
            }

            var postDtos = _mapper.Map<IEnumerable<CommunityPostDto>>(posts);

            var postIds = posts.Select(p => p.Id).ToList();

            var likeCounts = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .Where(pl => pl.PostId.HasValue && postIds.Contains(pl.PostId.Value))
                .GroupBy(pl => pl.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Likes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Likes));

            var unlikeCounts = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .Where(pu => pu.PostId.HasValue && postIds.Contains(pu.PostId.Value))
                .GroupBy(pu => pu.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Unlikes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Unlikes));

            var commentCounts = (await _unitOfWork.Repository<PostComment>().GetAllAsync())
                .Where(c => c.PostId.HasValue && postIds.Contains(c.PostId.Value))
                .GroupBy(c => c.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            var shareCounts = (await _unitOfWork.Repository<PostShare>().GetAllAsync())
                .Where(s => s.PostId.HasValue && postIds.Contains(s.PostId.Value))
                .GroupBy(s => s.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            foreach (var postDto in postDtos)
            {
                postDto.LikeCount = likeCounts.ContainsKey(postDto.Id) ? likeCounts[postDto.Id].Count : 0;
                postDto.UnlikeCount = unlikeCounts.ContainsKey(postDto.Id) ? unlikeCounts[postDto.Id].Count : 0;
                postDto.CommentCount = commentCounts.ContainsKey(postDto.Id) ? commentCounts[postDto.Id] : 0;
                postDto.ShareCount = shareCounts.ContainsKey(postDto.Id) ? shareCounts[postDto.Id] : 0;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    postDto.IsLiked = likeCounts.ContainsKey(postDto.Id) && likeCounts[postDto.Id].Likes.Any(l => l.UserId == currentUserId);
                    postDto.IsUnliked = unlikeCounts.ContainsKey(postDto.Id) && unlikeCounts[postDto.Id].Unlikes.Any(u => u.UserId == currentUserId);
                }
            }

            return postDtos;
        }

        public async Task<IEnumerable<CommunityPostDto>> GetUserPostsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");

            var posts = (await _unitOfWork.Repository<CommunityPost>().GetAllWithIncludeAsync(
                p => p.User,
                p => p.Community))
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (!posts.Any())
            {
                return new List<CommunityPostDto>();
            }

            var postDtos = _mapper.Map<IEnumerable<CommunityPostDto>>(posts);

            var postIds = posts.Select(p => p.Id).ToList();

            var likeCounts = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .Where(pl => pl.PostId.HasValue && postIds.Contains(pl.PostId.Value))
                .GroupBy(pl => pl.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Likes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Likes));

            var unlikeCounts = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .Where(pu => pu.PostId.HasValue && postIds.Contains(pu.PostId.Value))
                .GroupBy(pu => pu.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Unlikes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Unlikes));

            var commentCounts = (await _unitOfWork.Repository<PostComment>().GetAllAsync())
                .Where(c => c.PostId.HasValue && postIds.Contains(c.PostId.Value))
                .GroupBy(c => c.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            var shareCounts = (await _unitOfWork.Repository<PostShare>().GetAllAsync())
                .Where(s => s.PostId.HasValue && postIds.Contains(s.PostId.Value))
                .GroupBy(s => s.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            foreach (var postDto in postDtos)
            {
                postDto.LikeCount = likeCounts.ContainsKey(postDto.Id) ? likeCounts[postDto.Id].Count : 0;
                postDto.UnlikeCount = unlikeCounts.ContainsKey(postDto.Id) ? unlikeCounts[postDto.Id].Count : 0;
                postDto.CommentCount = commentCounts.ContainsKey(postDto.Id) ? commentCounts[postDto.Id] : 0;
                postDto.ShareCount = shareCounts.ContainsKey(postDto.Id) ? shareCounts[postDto.Id] : 0;

                if (!string.IsNullOrEmpty(userId))
                {
                    postDto.IsLiked = likeCounts.ContainsKey(postDto.Id) && likeCounts[postDto.Id].Likes.Any(l => l.UserId == userId);
                    postDto.IsUnliked = unlikeCounts.ContainsKey(postDto.Id) && unlikeCounts[postDto.Id].Unlikes.Any(u => u.UserId == userId);
                }
            }

            return postDtos;
        }

        public async Task<IEnumerable<CommunityPostDto>> GetAllCommunityPostsAsync(int pageNumber = 1, int pageSize = 20, string currentUserId = null)
        {
            var cacheKey = $"CommunityPosts_{pageNumber}_{pageSize}_{currentUserId}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<CommunityPostDto> cachedPosts))
            {
                return cachedPosts;
            }

            var allPosts = (await _unitOfWork.Repository<CommunityPost>().GetAllWithIncludeAsync(
                p => p.User,
                p => p.Community))
                .Where(p => p.CommunityId.HasValue)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (!allPosts.Any())
            {
                return new List<CommunityPostDto>();
            }

            var totalPosts = allPosts.Count;
            var totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var paginatedPosts = allPosts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var postDtos = _mapper.Map<IEnumerable<CommunityPostDto>>(paginatedPosts);

            var postIds = paginatedPosts.Select(p => p.Id).ToList();

            var likeCounts = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .Where(pl => pl.PostId.HasValue && postIds.Contains(pl.PostId.Value))
                .GroupBy(pl => pl.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Likes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Likes));

            var unlikeCounts = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .Where(pu => pu.PostId.HasValue && postIds.Contains(pu.PostId.Value))
                .GroupBy(pu => pu.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count(), Unlikes = g.ToList() })
                .ToDictionary(x => x.PostId, x => (x.Count, x.Unlikes));

            var commentCounts = (await _unitOfWork.Repository<PostComment>().GetAllAsync())
                .Where(c => c.PostId.HasValue && postIds.Contains(c.PostId.Value))
                .GroupBy(c => c.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            var shareCounts = (await _unitOfWork.Repository<PostShare>().GetAllAsync())
                .Where(s => s.PostId.HasValue && postIds.Contains(s.PostId.Value))
                .GroupBy(s => s.PostId.Value)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.PostId, x => x.Count);

            foreach (var postDto in postDtos)
            {
                postDto.LikeCount = likeCounts.ContainsKey(postDto.Id) ? likeCounts[postDto.Id].Count : 0;
                postDto.UnlikeCount = unlikeCounts.ContainsKey(postDto.Id) ? unlikeCounts[postDto.Id].Count : 0;
                postDto.CommentCount = commentCounts.ContainsKey(postDto.Id) ? commentCounts[postDto.Id] : 0;
                postDto.ShareCount = shareCounts.ContainsKey(postDto.Id) ? shareCounts[postDto.Id] : 0;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    postDto.IsLiked = likeCounts.ContainsKey(postDto.Id) && likeCounts[postDto.Id].Likes.Any(l => l.UserId == currentUserId);
                    postDto.IsUnliked = unlikeCounts.ContainsKey(postDto.Id) && unlikeCounts[postDto.Id].Unlikes.Any(u => u.UserId == currentUserId);
                }
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            _cache.Set(cacheKey, postDtos, cacheOptions);

            return postDtos;
        }

        public async Task<IEnumerable<CommunityPostSummaryDto>> GetAllCommunityPostsSummaryAsync(int pageNumber = 1, int pageSize = 20)
        {
            var cacheKey = $"CommunityPostsSummary_{pageNumber}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<CommunityPostSummaryDto> cachedPosts))
            {
                return cachedPosts;
            }

            var allPosts = (await _unitOfWork.Repository<CommunityPost>().GetAllWithIncludeAsync(
                p => p.User,
                p => p.Community))
                .Where(p => p.CommunityId.HasValue)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (!allPosts.Any())
            {
                return new List<CommunityPostSummaryDto>();
            }

            var totalPosts = allPosts.Count;
            var totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var paginatedPosts = allPosts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            
            var postDtos = paginatedPosts.Select(p => new CommunityPostSummaryDto
            {
                Id = p.Id.ToString(), 
                Content = p.Content,
                CommunityId = p.CommunityId, 
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd"),
                UserId = p.UserId 
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            _cache.Set(cacheKey, postDtos, cacheOptions);

            return postDtos;
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
                PostId = dto.PostId,
                CreatedAt = DateTime.UtcNow,
                ImageUrl = string.Empty
            };

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "post-images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                var filePath = Path.Combine(imagesFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                comment.ImageUrl = $"/post-images/{uniqueFileName}";
            }

            await _unitOfWork.Repository<PostComment>().AddAsync(comment);

            post.CommentCount = (post.CommentCount ?? 0) + 1;
            _unitOfWork.Repository<CommunityPost>().Update(post);

            await _unitOfWork.CountAsync();

            var commentDto = _mapper.Map<PostCommentDto>(comment);
            commentDto.UserName = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            return commentDto;
        }

        public async Task<IEnumerable<PostCommentDto>> GetPostCommentsAsync(long postId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            var comments = (await _unitOfWork.Repository<PostComment>().GetAllWithIncludeAsync(
                c => c.User))
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var commentDtos = _mapper.Map<IEnumerable<PostCommentDto>>(comments);
            return commentDtos;
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

            var existingUnlike = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .FirstOrDefault(pu => pu.PostId == postId && pu.UserId == userId);

            if (existingUnlike != null)
            {
                _unitOfWork.Repository<PostUnlike>().Delete(existingUnlike);
            }

            var like = new PostLike
            {
                UserId = userId,
                PostId = postId
            };

            await _unitOfWork.Repository<PostLike>().AddAsync(like);
            await _unitOfWork.CountAsync();
        }

        public async Task UnlikePostAsync(long postId, string userId)
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
                _unitOfWork.Repository<PostLike>().Delete(existingLike);
                await AddUnlikeAsync(postId, userId);
            }
            else
            {
                var existingUnlike = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                    .FirstOrDefault(pu => pu.PostId == postId && pu.UserId == userId);

                if (existingUnlike != null)
                {
                    throw new Exception("User has already unliked this post.");
                }

                await AddUnlikeAsync(postId, userId);
            }

            await _unitOfWork.CountAsync();
        }

        private async Task AddUnlikeAsync(long postId, string userId)
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

            var existingUnlike = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .FirstOrDefault(pu => pu.PostId == postId && pu.UserId == userId);

            if (existingUnlike != null)
            {
                throw new Exception("User has already unliked this post.");
            }

            var unlike = new PostUnlike
            {
                UserId = userId,
                PostId = postId
            };
            await _unitOfWork.Repository<PostUnlike>().AddAsync(unlike);
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
                long communityId = sharedWithCommunityId.Value;
                var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
                if (community == null)
                {
                    throw new Exception("Community not found.");
                }

                var isMember = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                    .Any(m => m.CommunityId == communityId && m.UserId == userId);

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

        public async Task AssignModeratorAsync(AssignModeratorDto dto, string adminId)
        {
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(dto.CommunityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            if (community.AdminId != adminId)
            {
                throw new Exception("Only the admin can assign moderators.");
            }

            var member = (await _unitOfWork.Repository<CommunityMember>().GetAllAsync())
                .FirstOrDefault(m => m.CommunityId == dto.CommunityId && m.UserId == dto.UserId);

            if (member == null)
            {
                throw new Exception("User is not a member of this community.");
            }

            if (member.IsModerator == true)
            {
                throw new Exception("User is already a moderator.");
            }

            member.IsModerator = true;
            _unitOfWork.Repository<CommunityMember>().Update(member);
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

            if (member.IsModerator != true)
            {
                throw new Exception("User is not a moderator.");
            }

            member.IsModerator = false;
            _unitOfWork.Repository<CommunityMember>().Update(member);
            await _unitOfWork.CountAsync();
        }

        public async Task DeletePostAsync(long postId, string requesterId)
        {
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            if (!post.CommunityId.HasValue)
            {
                throw new Exception("Post is not associated with a community.");
            }
            long communityId = post.CommunityId.Value;
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
            if (community == null)
            {
                throw new Exception("Community not found.");
            }

            var isAuthor = post.UserId == requesterId;
            var isAdmin = community.AdminId == requesterId;
            var isModerator = false;
            var isMember = false;

            var members = await _unitOfWork.Repository<CommunityMember>().GetAllAsync();
            var member = members.FirstOrDefault(m => m.CommunityId == post.CommunityId && m.UserId == requesterId);

            if (member != null)
            {
                isMember = true;
                if (!isAdmin)
                {
                    isModerator = member.IsModerator ?? false;
                }
            }

            if (!isMember)
            {
                throw new Exception("You must be a member of the community to delete posts.");
            }

            if (!isAuthor && !isAdmin && !isModerator)
            {
                throw new Exception($"You don't have permission to delete this post. " +
                    $"Details: Author={isAuthor}, Admin={isAdmin}, Moderator={isModerator}, " +
                    $"PostCommunityId={post.CommunityId}, RequesterId={requesterId}");
            }

            var comments = (await _unitOfWork.Repository<PostComment>().GetAllAsync())
                .Where(c => c.PostId == postId)
                .ToList();
            foreach (var comment in comments)
            {
                if (!string.IsNullOrEmpty(comment.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", comment.ImageUrl.TrimStart('/'));
                    if (File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                }
                _unitOfWork.Repository<PostComment>().Delete(comment);
            }
            if (comments.Any())
            {
                await _unitOfWork.CountAsync();
            }

            var likes = (await _unitOfWork.Repository<PostLike>().GetAllAsync())
                .Where(l => l.PostId == postId)
                .ToList();
            foreach (var like in likes)
            {
                _unitOfWork.Repository<PostLike>().Delete(like);
            }
            if (likes.Any())
            {
                await _unitOfWork.CountAsync();
            }

            var unlikes = (await _unitOfWork.Repository<PostUnlike>().GetAllAsync())
                .Where(u => u.PostId == postId)
                .ToList();
            foreach (var unlike in unlikes)
            {
                _unitOfWork.Repository<PostUnlike>().Delete(unlike);
            }
            if (unlikes.Any())
            {
                await _unitOfWork.CountAsync();
            }

            var shares = (await _unitOfWork.Repository<PostShare>().GetAllAsync())
                .Where(s => s.PostId == postId)
                .ToList();
            foreach (var share in shares)
            {
                _unitOfWork.Repository<PostShare>().Delete(share);
            }
            if (shares.Any())
            {
                await _unitOfWork.CountAsync();
            }

            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            community.PostCount = (community.PostCount ?? 0) > 0 ? community.PostCount - 1 : 0;
            _unitOfWork.Repository<Community>().Update(community);

            _unitOfWork.Repository<CommunityPost>().Delete(post);
            await _unitOfWork.CountAsync();
        }

        public async Task DeleteCommentAsync(long commentId, string requesterId)
        {
            var comment = await _unitOfWork.Repository<PostComment>().GetByIdAsync(commentId);
            if (comment == null)
            {
                throw new Exception("Comment not found.");
            }

            if (!comment.PostId.HasValue)
            {
                throw new Exception("Comment is not associated with a post.");
            }

            long postId = comment.PostId.Value;
            var post = await _unitOfWork.Repository<CommunityPost>().GetByIdAsync(postId);
            if (post == null)
            {
                throw new Exception("Post not found.");
            }

            if (!post.CommunityId.HasValue)
            {
                throw new Exception("Post is not associated with a community.");
            }

            long communityId = post.CommunityId.Value;
            var community = await _unitOfWork.Repository<Community>().GetByIdAsync(communityId);
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
                    .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == requesterId);

                isModerator = member?.IsModerator ?? false;
            }

            if (!isCommentAuthor && !isPostAuthor && !isAdmin && !isModerator)
            {
                throw new Exception("You don't have permission to delete this comment.");
            }

            if (!string.IsNullOrEmpty(comment.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", comment.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            post.CommentCount = (post.CommentCount ?? 0) > 0 ? post.CommentCount - 1 : 0;
            _unitOfWork.Repository<CommunityPost>().Update(post);

            _unitOfWork.Repository<PostComment>().Delete(comment);
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
                .FirstOrDefault(m => m.CommunityId == communityId && m.UserId == userId);

            if (targetMember != null && targetMember.IsModerator == true && !isAdmin)
            {
                throw new Exception("Only the admin can ban other moderators.");
            }

            if (targetMember != null)
            {
                _unitOfWork.Repository<CommunityMember>().Delete(targetMember);
                await _unitOfWork.CountAsync();
            }
        }
        public async Task<IEnumerable<CommunityPostsCountDto>> GetCommunityPostsCountAsync()
        {
            var communities = await _unitOfWork.Repository<Community>().GetAllAsync();
            var posts = await _unitOfWork.Repository<CommunityPost>().GetAllAsync();

            var communityPostsCount = communities.Select(c => new CommunityPostsCountDto
            {
                CommunityID = c.Id.ToString(),
                PostCount = posts.Count(p => p.CommunityId.HasValue && p.CommunityId.Value == c.Id)
            }).ToList();

            return communityPostsCount;
        }
        public async Task<IEnumerable<CommunitySummaryDto>> GetAllCommunitiesSummaryAsync()
        {
            var communities = await _unitOfWork.Repository<Community>().GetAllAsync();

            var communitySummaries = communities.Select(c => new CommunitySummaryDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description ?? "No description available"
            }).ToList();

            return communitySummaries;
        }
        public async Task<MonthlyVisitsDto> GetMonthlyVisitsAsync(int year)
        {
            var visits = await _unitOfWork.Repository<Visit>().GetAllAsync();
            var yearVisits = visits.Where(v => v.VisitDate.Year == year).ToList();

            var monthlyVisits = new MonthlyVisitsDto
            {
                Year = year.ToString(),
                Jan = yearVisits.Count(v => v.VisitDate.Month == 1),
                Feb = yearVisits.Count(v => v.VisitDate.Month == 2),
                Mar = yearVisits.Count(v => v.VisitDate.Month == 3),
                April = yearVisits.Count(v => v.VisitDate.Month == 4),
                May = yearVisits.Count(v => v.VisitDate.Month == 5),
                June = yearVisits.Count(v => v.VisitDate.Month == 6),
                July = yearVisits.Count(v => v.VisitDate.Month == 7),
                Aug = yearVisits.Count(v => v.VisitDate.Month == 8),
                Sept = yearVisits.Count(v => v.VisitDate.Month == 9),
                Oct = yearVisits.Count(v => v.VisitDate.Month == 10),
                Nov = yearVisits.Count(v => v.VisitDate.Month == 11),
                Dec = yearVisits.Count(v => v.VisitDate.Month == 12)
            };

            return monthlyVisits;
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
                IsModerator = false,
                JoinedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CommunityMember>().AddAsync(member);
            await _unitOfWork.CountAsync();

            return true;
        }
    }
}