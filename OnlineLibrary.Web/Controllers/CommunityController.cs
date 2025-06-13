﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.CommunityService;
using OnlineLibrary.Service.CommunityService.Dtos;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Web.Hubs.Dtos;
using OnlineLibrary.Service.ContentModerationService;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : BaseController
    {
        private readonly ICommunityService _communityService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly OnlineLibraryIdentityDbContext _dbContext;
        private readonly IContentModerationService _contentModerationService;

        public CommunityController(
            ICommunityService communityService,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> notificationHub,
            OnlineLibraryIdentityDbContext dbContext,
            IContentModerationService contentModerationService)
        {
            _communityService = communityService;
            _userManager = userManager;
            _notificationHub = notificationHub;
            _dbContext = dbContext;
            _contentModerationService = contentModerationService;
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private async Task<(string Username, string ProfilePicture)> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userProfile = await _dbContext.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            string username = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            string profilePicture = userProfile?.ProfilePhoto ?? "default_profile.jpg";

            return (username, profilePicture);
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var minutes = (int)(DateTime.UtcNow - createdAt).TotalMinutes;
            if (minutes < 60)
                return $"{minutes} min ago";
            var hours = (int)(DateTime.UtcNow - createdAt).TotalHours;
            if (hours < 24)
                return $"{hours} h ago";
            var days = (int)(DateTime.UtcNow - createdAt).TotalDays;
            return $"{days} d ago";
        }

        [HttpGet("notifications/latest")]
        [Authorize]
        public async Task<IActionResult> GetLatestNotifications()
        {
            var userId = GetUserId();
            var notifications = await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    NotificationType = n.NotificationType,
                    Message = n.Message,
                    ActorUserId = n.ActorUserId,
                    ActorUserName = n.ActorUserName,
                    ActorProfilePicture = n.ActorProfilePicture,
                    RelatedEntityId = n.RelatedEntityId,
                    CreatedAt = n.CreatedAt,
                    TimeAgo = EF.Functions.DateDiffMinute(n.CreatedAt, DateTime.UtcNow) < 60
                        ? EF.Functions.DateDiffMinute(n.CreatedAt, DateTime.UtcNow) + " min ago"
                        : EF.Functions.DateDiffHour(n.CreatedAt, DateTime.UtcNow) < 24
                            ? EF.Functions.DateDiffHour(n.CreatedAt, DateTime.UtcNow) + " h ago"
                            : EF.Functions.DateDiffDay(n.CreatedAt, DateTime.UtcNow) + " d ago"
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommunityDto>>> GetAllCommunities()
        {
            var userId = GetUserId();
            var communities = (await _communityService.GetAllCommunitiesAsync(userId)).ToList();

            var communityIds = communities.Select(c => c.Id).ToList();
            var images = await _dbContext.CommunityImages
                .Where(i => communityIds.Contains(i.CommunityId))
                .GroupBy(i => i.CommunityId)
                .Select(g => g.OrderByDescending(i => i.CreatedAt).FirstOrDefault())
                .ToListAsync();

            foreach (var community in communities)
            {
                var image = images.FirstOrDefault(img => img.CommunityId == community.Id);
                community.ImageUrl = image?.ImageUrl;
            }

            return Ok(communities);
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CommunityDto>> GetCommunity(long id)
        {
            var visit = new Visit
            {
                VisitDate = DateTime.UtcNow,
                UserId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null
            };
            _dbContext.Visits.Add(visit);
            await _dbContext.SaveChangesAsync();

            var community = await _communityService.GetCommunityByIdAsync(id);
            if (community == null)
                return NotFound();

            var image = await _dbContext.CommunityImages
                .Where(i => i.CommunityId == id)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            community.ImageUrl = image?.ImageUrl;

            var userId = GetUserId();
            var communityMembers = await _communityService.GetCommunityMembersAsync(id);
            community.IsMember = communityMembers.Any(m => m.UserId == userId);

            return Ok(community);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CommunityDto>> CreateCommunity([FromForm] CreateCommunityDto dto)
        {
            var userId = GetUserId();
            try
            {
                var community = await _communityService.CreateCommunityAsync(dto, userId);

                string? imageUrl = null;

                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "community-images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{dto.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(stream);
                    }

                    imageUrl = $"/community-images/{uniqueFileName}";

                    var image = new CommunityImage
                    {
                        CommunityId = community.Id,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.CommunityImages.Add(image);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    imageUrl = null;
                }

                var response = new CommunityDto
                {
                    Id = community.Id,
                    Name = community.Name,
                    Description = community.Description,
                    CreatedAt = community.CreatedAt,
                    MemberCount = community.MemberCount,
                    PostCount = community.PostCount,
                    IsMember = community.IsMember,
                    AdminId = community.AdminId,
                    AdminName = community.AdminName,
                    ImageUrl = imageUrl
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{communityId}/join")]
        [Authorize]
        public async Task<IActionResult> JoinCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.JoinCommunityAsync(communityId, userId);
            return Ok();
        }

        [HttpPost("{communityId}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveCommunity(long communityId)
        {
            var userId = GetUserId();
            await _communityService.LeaveCommunityAsync(communityId, userId);
            return Ok();
        }

        [HttpPost("posts")]
        [Authorize(Roles = "Sender,Admin,Moderator,User")]
        public async Task<ActionResult<CommunityPostDto>> CreatePost([FromForm] CreatePostDto dto)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var userId = GetUserId();
            var (username, profilePicture) = await GetUserDetails(userId);

            var moderationResult = await _contentModerationService.ModerateTextAsync(dto.Content);

            string notificationType, notificationMessage;
            if (!moderationResult.IsAppropriate)
            {
                notificationType = NotificationTypes.PostRejected;
                notificationMessage = "Your post was rejected due to inappropriate content.";

                var notification = new Notification
                {
                    UserId = userId,
                    ActorUserId = userId,
                    ActorUserName = username,
                    ActorProfilePicture = profilePicture,
                    NotificationType = notificationType,
                    Message = notificationMessage,
                    RelatedEntityId = null,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    NotificationType = notification.NotificationType,
                    Message = notification.Message,
                    ActorUserId = notification.ActorUserId,
                    ActorUserName = notification.ActorUserName,
                    ActorProfilePicture = notification.ActorProfilePicture,
                    RelatedEntityId = notification.RelatedEntityId,
                    CreatedAt = notification.CreatedAt,
                    TimeAgo = GetTimeAgo(notification.CreatedAt)
                };
                await _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", notificationDto);

                return BadRequest(new
                {
                    error = notificationMessage,
                    details = moderationResult.ReasonMessage,
                    category = moderationResult.Category
                });
            }

            var post = await _communityService.CreatePostAsync(dto, userId);

            notificationType = NotificationTypes.PostAccepted;
            notificationMessage = "Your post has been accepted and published in the community.";

            var acceptedNotification = new Notification
            {
                UserId = userId,
                ActorUserId = userId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                NotificationType = notificationType,
                Message = notificationMessage,
                RelatedEntityId = post != null ? post.CommunityId : null,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Notifications.Add(acceptedNotification);
            await _dbContext.SaveChangesAsync();

            var acceptedNotificationDto = new NotificationDto
            {
                Id = acceptedNotification.Id,
                NotificationType = acceptedNotification.NotificationType,
                Message = acceptedNotification.Message,
                ActorUserId = acceptedNotification.ActorUserId,
                ActorUserName = acceptedNotification.ActorUserName,
                ActorProfilePicture = acceptedNotification.ActorProfilePicture,
                RelatedEntityId = acceptedNotification.RelatedEntityId,
                CreatedAt = acceptedNotification.CreatedAt,
                TimeAgo = GetTimeAgo(acceptedNotification.CreatedAt)
            };
            await _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", acceptedNotificationDto);

            var groupNotificationDto = new NotificationDto
            {
                Id = 0, // إشعار عام للمجموعة، يمكن تجاهل Id أو وضع قيمة مناسبة
                NotificationType = NotificationTypes.PostAccepted,
                Message = $"{username} added a new post to the community!",
                ActorUserId = userId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                RelatedEntityId = post?.CommunityId,
                CreatedAt = DateTime.UtcNow,
                TimeAgo = GetTimeAgo(DateTime.UtcNow)
            };

            await _notificationHub.Clients.GroupExcept($"Community_{dto.CommunityId}", userId)
                .SendAsync("ReceiveNotification", groupNotificationDto);

            return Ok(post);
        }

        [HttpGet("{communityId}/posts")]
        [Authorize(Roles = "Receiver,Admin,Moderator,User")]
        public async Task<ActionResult<IEnumerable<CommunityPostDto>>> GetCommunityPosts(long communityId)
        {
            var userId = GetUserId();
            var posts = await _communityService.GetCommunityPostsAsync(communityId, userId);
            return Ok(posts);
        }

        [HttpGet("all-posts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CommunityPostDto>>> GetAllPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var posts = await _communityService.GetAllCommunityPostsAsync(pageNumber, pageSize, userId);
            return Ok(posts);
        }

        [HttpPost("posts/{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(long postId)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(postId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            await _communityService.LikePostAsync(postId, userId);

            if (userId != postOwnerId)
            {
                var (username, profilePicture) = await GetUserDetails(userId);

                var notification = new Notification
                {
                    UserId = postOwnerId,
                    ActorUserId = userId,
                    ActorUserName = username,
                    ActorProfilePicture = profilePicture,
                    NotificationType = NotificationTypes.PostLike,
                    Message = $"{username} liked your post!",
                    RelatedEntityId = postId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    NotificationType = notification.NotificationType,
                    Message = notification.Message,
                    ActorUserId = notification.ActorUserId,
                    ActorUserName = notification.ActorUserName,
                    ActorProfilePicture = notification.ActorProfilePicture,
                    RelatedEntityId = notification.RelatedEntityId,
                    CreatedAt = notification.CreatedAt,
                    TimeAgo = GetTimeAgo(notification.CreatedAt)
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notificationDto);
            }

            return Ok();
        }

        [HttpPost("posts/{postId}/unlike")]
        [Authorize]
        public async Task<IActionResult> UnlikePost(long postId)
        {
            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(postId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            await _communityService.UnlikePostAsync(postId, userId);

            if (userId != postOwnerId)
            {
                var (username, profilePicture) = await GetUserDetails(userId);
                var notification = new Notification
                {
                    UserId = postOwnerId,
                    ActorUserId = userId,
                    ActorUserName = username,
                    ActorProfilePicture = profilePicture,
                    NotificationType = NotificationTypes.PostUnlike,
                    Message = $"{username} unliked your post!",
                    RelatedEntityId = postId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    NotificationType = notification.NotificationType,
                    Message = notification.Message,
                    ActorUserId = notification.ActorUserId,
                    ActorUserName = notification.ActorUserName,
                    ActorProfilePicture = notification.ActorProfilePicture,
                    RelatedEntityId = notification.RelatedEntityId,
                    CreatedAt = notification.CreatedAt,
                    TimeAgo = GetTimeAgo(notification.CreatedAt)
                };

                await _notificationHub.Clients.User(postOwnerId)
                    .SendAsync("ReceiveNotification", notificationDto);
            }

            return Ok();
        }

        [HttpPost("posts/comments")]
        [Authorize]
        public async Task<ActionResult<PostCommentDto>> AddComment(CreateCommentDto dto)
        {
            var moderationResult = await _contentModerationService.ModerateTextAsync(dto.Content);

            if (!moderationResult.IsAppropriate)
            {
                return BadRequest(new
                {
                    error = "Your comment was rejected due to inappropriate content.",
                    details = moderationResult.ReasonMessage,
                    category = moderationResult.Category
                });
            }

            var userId = GetUserId();
            string postOwnerId = await GetPostOwnerId(dto.PostId);
            if (string.IsNullOrEmpty(postOwnerId))
            {
                return NotFound("Post not found");
            }

            var comment = await _communityService.AddCommentAsync(dto, userId);

            var (username, profilePicture) = await GetUserDetails(userId);

            var notification = new Notification
            {
                UserId = postOwnerId,
                ActorUserId = userId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                NotificationType = NotificationTypes.PostComment,
                Message = $"{username} commented on your post!",
                RelatedEntityId = dto.PostId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Message = notification.Message,
                ActorUserId = notification.ActorUserId,
                ActorUserName = notification.ActorUserName,
                ActorProfilePicture = notification.ActorProfilePicture,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };

            await _notificationHub.Clients.User(postOwnerId)
                .SendAsync("ReceiveNotification", notificationDto);

            return Ok(comment);
        }

        [HttpGet("posts/{postId}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PostCommentDto>>> GetPostComments(long postId)
        {
            var comments = await _communityService.GetPostCommentsAsync(postId);
            return Ok(comments);
        }


        [HttpPost("moderators/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignModerator(AssignModeratorDto dto)
        {
            var adminId = GetUserId();
            await _communityService.AssignModeratorAsync(dto, adminId);

            var (username, profilePicture) = await GetUserDetails(dto.UserId);
            var notification = new Notification
            {
                UserId = dto.UserId,
                ActorUserId = adminId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                NotificationType = "ModeratorAssigned",
                Message = $"{username} has been assigned as a moderator in the community!",
                RelatedEntityId = dto.CommunityId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Message = notification.Message,
                ActorUserId = notification.ActorUserId,
                ActorUserName = notification.ActorUserName,
                ActorProfilePicture = notification.ActorProfilePicture,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };

            await _notificationHub.Clients.User(dto.UserId)
                .SendAsync("ReceiveNotification", notificationDto);

            return Ok();
        }

        [HttpGet("moderators/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllModerators()
        {
            var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
            var result = moderators.Select(user => new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.firstName,
                user.LastName
            });

            return Ok(result);
        }

        [HttpPost("moderators/remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveModerator([FromQuery] long communityId, [FromQuery] string userId)
        {
            var adminId = GetUserId();
            await _communityService.RemoveModeratorAsync(communityId, userId, adminId);

            var (username, profilePicture) = await GetUserDetails(userId);
            var notification = new Notification
            {
                UserId = userId,
                ActorUserId = adminId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                NotificationType = "ModeratorRemoved",
                Message = $"{username} has been removed as a moderator from the community!",
                RelatedEntityId = communityId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Message = notification.Message,
                ActorUserId = notification.ActorUserId,
                ActorUserName = notification.ActorUserName,
                ActorProfilePicture = notification.ActorProfilePicture,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedAt = notification.CreatedAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };

            await _notificationHub.Clients.User(userId)
                .SendAsync("ReceiveNotification", notificationDto);

            return Ok();
        }

        [HttpDelete("posts/{postId}")]
        [Authorize(Roles = "Admin,User,Moderator")]
        public async Task<IActionResult> DeletePost(long postId)
        {
            var requesterId = GetUserId();
            await _communityService.DeletePostAsync(postId, requesterId);
            return Ok();
        }

        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(long commentId)
        {
            var requesterId = GetUserId();
            await _communityService.DeleteCommentAsync(commentId, requesterId);
            return Ok();
        }

        [HttpPost("{communityId}/ban/{userId}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> BanUser(long communityId, string userId)
        {
            var requesterId = GetUserId();
            await _communityService.BanUserAsync(communityId, userId, requesterId);
            return Ok();
        }

        [HttpPost("{communityId}/unban/{userId}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UnbanUser(long communityId, string userId)
        {
            var requesterId = GetUserId();
            var result = await _communityService.UnbanUserAsync(communityId, requesterId, userId);
            return Ok(new { Unbanned = result });
        }

        private async Task<string> GetPostOwnerId(long postId)
        {
            var post = await _dbContext.CommunityPosts.FindAsync(postId);
            return post?.UserId ?? string.Empty;
        }

        [HttpPost("{communityId}/images/upload")]
        public async Task<IActionResult> UploadCommunityImage(long communityId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a valid image.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "community-images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/community-images/{uniqueFileName}";

            var image = new CommunityImage
            {
                CommunityId = communityId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.CommunityImages.Add(image);
            await _dbContext.SaveChangesAsync();

            return Ok(image);
        }

        [HttpPut("{communityId}/images/update")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UpdateCommunityImage(long communityId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a valid image.");

            var image = await _dbContext.CommunityImages.FirstOrDefaultAsync(i => i.CommunityId == communityId);
            if (image == null)
                return NotFound("Image not found.");

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "community-images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            image.ImageUrl = $"/community-images/{uniqueFileName}";
            image.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(image);
        }

        [HttpDelete("images/{imageId}/delete")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteCommunityImage(long imageId)
        {
            var image = await _dbContext.CommunityImages.FindAsync(imageId);
            if (image == null)
                return NotFound("Image not found.");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _dbContext.CommunityImages.Remove(image);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Image deleted successfully." });
        }
    }
}
