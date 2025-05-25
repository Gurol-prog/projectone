using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projectone.Dtos;
using projectone.Models;
using projectone.Services;

namespace projectone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContentController : ControllerBase
    {
        private readonly ContentService _contentService;

        public ContentController(ContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Content>>> GetAll()
        {
            var contents = await _contentService.GetAllAsync();
            return Ok(contents);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(string id)
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
                return NotFound();

            // Eğer kullanıcı giriş yapmışsa, beğeni durumunu da gönder
            string? userLikeStatus = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    userLikeStatus = await _contentService.GetUserLikeStatusAsync(userId, id);
                }
            }

            return Ok(new
            {
                Content = content,
                UserLikeStatus = userLikeStatus // "like", "dislike" veya null
            });
        }

        [HttpPost]
        public async Task<ActionResult<Content>> CreateContent(ContentCreateDto contentDto)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı kimliği bulunamadı");

            var content = new Content
            {
                UserId = userId,
                ContentName = contentDto.ContentName,
                ContentDescription = contentDto.ContentDescription,
                ContentText = contentDto.ContentText,
                ContentUrl = contentDto.ContentUrl,
                ContentType = contentDto.ContentType,
                InsertTime = DateTime.UtcNow,
                LikeCount = 0,
                DislikeCount = 0
            };

            await _contentService.CreateContentAsync(content);
            return CreatedAtAction(nameof(GetById), new { id = content.Id }, content);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteContent(string id)
        {
            var existingContent = await _contentService.GetByIdAsync(id);
            if (existingContent == null)
                return NotFound();

            var userId = User.FindFirst("userId")?.Value;
            if (existingContent.UserId != userId)
                return Forbid("Bu içeriği silme yetkiniz yok");

            await _contentService.SoftDeleteContentAsync(id);
            return NoContent();
        }

        // 🔥 YENİ TOGGLE SİSTEMİ - Tek endpoint ile like/dislike
        [HttpPost("{id}/like")]
        public async Task<ActionResult> ToggleLike(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _contentService.ToggleLikeAsync(userId, id, "like");
            
            if (!result.success)
                return BadRequest(result.message);

            return Ok(new
            {
                Message = result.message,
                LikeCount = result.likeCount,
                DislikeCount = result.dislikeCount,
                UserLikeStatus = await _contentService.GetUserLikeStatusAsync(userId, id)
            });
        }

        [HttpPost("{id}/dislike")]
        public async Task<ActionResult> ToggleDislike(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _contentService.ToggleLikeAsync(userId, id, "dislike");
            
            if (!result.success)
                return BadRequest(result.message);

            return Ok(new
            {
                Message = result.message,
                LikeCount = result.likeCount,
                DislikeCount = result.dislikeCount,
                UserLikeStatus = await _contentService.GetUserLikeStatusAsync(userId, id)
            });
        }

        // Kullanıcının beğeni durumunu kontrol et
        [HttpGet("{id}/like-status")]
        public async Task<ActionResult> GetLikeStatus(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var status = await _contentService.GetUserLikeStatusAsync(userId, id);
            return Ok(new { UserLikeStatus = status });
        }

        // Popüler içerikler
        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Content>>> GetPopularContents(int limit = 10)
        {
            var contents = await _contentService.GetMostLikedContentsAsync(limit);
            return Ok(contents);
        }

        // Trend içerikler
        [HttpGet("trending")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Content>>> GetTrendingContents(int limit = 10)
        {
            var contents = await _contentService.GetTrendingContentsAsync(limit);
            return Ok(contents);
        }
    }
}