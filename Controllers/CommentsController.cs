using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projectone.Dtos;
using projectone.Models;
using projectone.Services;

namespace projectone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ContentCommentsService _commentsService;
        private readonly ContentService _contentService;
        private readonly UsersService _usersService;
        private readonly UserLikeCommentService _userLikeCommentService;

        public CommentsController(
            ContentCommentsService commentsService, 
            ContentService contentService,
            UsersService usersService,
            UserLikeCommentService userLikeCommentService)
        {
            _commentsService = commentsService;
            _contentService = contentService;
            _usersService = usersService;
            _userLikeCommentService = userLikeCommentService;
        }

        // Belirli bir içeriğin yorumlarını getir
        [HttpGet("content/{contentId}")]
        [AllowAnonymous] // Yorumları okumak için giriş gerekmez
        public async Task<ActionResult<List<CommentResponseDto>>> GetCommentsByContentId(string contentId)
        {
            // İçeriğin var olduğunu kontrol et
            var content = await _contentService.GetByIdAsync(contentId);
            if (content == null)
                return NotFound("İçerik bulunamadı");

            var comments = await _commentsService.GetCommentsWithRepliesAsync(contentId);
            var currentUserId = User.FindFirst("userId")?.Value;

            var responseDtos = new List<CommentResponseDto>();
            
            foreach (var comment in comments)
            {
                var commentDto = await MapToResponseDto(comment, currentUserId);
                
                // Reply'leri de map et
                if (comment.Replies != null && comment.Replies.Any())
                {
                    commentDto.Replies = new List<CommentResponseDto>();
                    foreach (var reply in comment.Replies)
                    {
                        var replyDto = await MapToResponseDto(reply, currentUserId);
                        commentDto.Replies.Add(replyDto);
                    }
                }
                
                responseDtos.Add(commentDto);
            }

            return Ok(responseDtos);
        }

        // Ana yorum oluştur
        [HttpPost("content/{contentId}")]
        public async Task<ActionResult<CommentResponseDto>> CreateComment(string contentId, CommentCreateDto commentDto)
        {
            // İçeriğin var olduğunu kontrol et
            var content = await _contentService.GetByIdAsync(contentId);
            if (content == null)
                return NotFound("İçerik bulunamadı");

            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var comment = new ContentComments
            {
                ContentId = contentId,
                UserId = userId,
                Comment = commentDto.Comment,
                ParentCommentId = null, // Ana yorum - parent yok
                InsertTime = DateTime.UtcNow
            };

            var createdComment = await _commentsService.CreateCommentAsync(comment);
            var responseDto = await MapToResponseDto(createdComment, userId);

            return CreatedAtAction(nameof(GetCommentById), new { id = createdComment.Id }, responseDto);
        }

        // Yoruma yanıt ver (YENİ ENDPOINT)
        [HttpPost("{commentId}/reply")]
        public async Task<ActionResult<CommentResponseDto>> ReplyToComment(string commentId, CommentCreateDto commentDto)
        {
            // Ana yorumun var olduğunu kontrol et
            var parentComment = await _commentsService.GetByIdAsync(commentId);
            if (parentComment == null)
                return NotFound("Ana yorum bulunamadı");

            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var replyComment = new ContentComments
            {
                ContentId = parentComment.ContentId, // Ana yorumun content ID'si
                UserId = userId,
                Comment = commentDto.Comment,
                ParentCommentId = commentId, // Ana yorumun ID'si
                InsertTime = DateTime.UtcNow
            };

            var createdReply = await _commentsService.CreateCommentAsync(replyComment);
            var responseDto = await MapToResponseDto(createdReply, userId);

            return CreatedAtAction(nameof(GetCommentById), new { id = createdReply.Id }, responseDto);
        }

        // Tek bir yorumu getir
        [HttpGet("{id}")]
        [AllowAnonymous] // Yorum okumak için giriş gerekmez
        public async Task<ActionResult<CommentResponseDto>> GetCommentById(string id)
        {
            var comment = await _commentsService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var currentUserId = User.FindFirst("userId")?.Value;
            var responseDto = await MapToResponseDto(comment, currentUserId);

            return Ok(responseDto);
        }

        // Yorum güncelle
        [HttpPut("{id}")]
        // Authorize zaten controller seviyesinde var
        public async Task<ActionResult> UpdateComment(string id, CommentUpdateDto commentDto)
        {
            var existingComment = await _commentsService.GetByIdAsync(id);
            if (existingComment == null)
                return NotFound();

            var userId = User.FindFirst("userId")?.Value;
            if (existingComment.UserId != userId)
                return Forbid("Bu yorumu güncelleme yetkiniz yok");

            var success = await _commentsService.UpdateCommentAsync(id, commentDto.Comment);
            if (!success)
                return BadRequest("Yorum güncellenemedi");

            return NoContent();
        }

        // Yorum sil
        [HttpDelete("{id}")]
        // Authorize zaten controller seviyesinde var
        public async Task<ActionResult> DeleteComment(string id)
        {
            var existingComment = await _commentsService.GetByIdAsync(id);
            if (existingComment == null)
                return NotFound();

            var userId = User.FindFirst("userId")?.Value;
            if (existingComment.UserId != userId)
                return Forbid("Bu yorumu silme yetkiniz yok");

            var success = await _commentsService.SoftDeleteCommentAsync(id);
            if (!success)
                return BadRequest("Yorum silinemedi");

            return NoContent();
        }

        // Yorum beğen/beğenme (Toggle sistemi)
        [HttpPost("{id}/like")]
        // Authorize zaten controller seviyesinde var
        public async Task<ActionResult> ToggleCommentLike(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _commentsService.ToggleCommentLikeAsync(userId, id, "like", _userLikeCommentService);
            
            if (!result.success)
                return BadRequest(result.message);

            return Ok(new
            {
                Message = result.message,
                LikeCount = result.likeCount,
                DislikeCount = result.dislikeCount,
                UserLikeStatus = await _commentsService.GetUserCommentLikeStatusAsync(userId, id, _userLikeCommentService)
            });
        }

        [HttpPost("{id}/dislike")]
        // Authorize zaten controller seviyesinde var
        public async Task<ActionResult> ToggleCommentDislike(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _commentsService.ToggleCommentLikeAsync(userId, id, "dislike", _userLikeCommentService);
            
            if (!result.success)
                return BadRequest(result.message);

            return Ok(new
            {
                Message = result.message,
                LikeCount = result.likeCount,
                DislikeCount = result.dislikeCount,
                UserLikeStatus = await _commentsService.GetUserCommentLikeStatusAsync(userId, id, _userLikeCommentService)
            });
        }

        // Kullanıcının yorum beğeni durumunu kontrol et
        [HttpGet("{id}/like-status")]
        // Authorize zaten controller seviyesinde var
        public async Task<ActionResult> GetCommentLikeStatus(string id)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var status = await _commentsService.GetUserCommentLikeStatusAsync(userId, id, _userLikeCommentService);
            return Ok(new { UserLikeStatus = status });
        }

        // En iyi yorumlar
        [HttpGet("content/{contentId}/top")]
        [AllowAnonymous] // Herkese açık
        public async Task<ActionResult<List<CommentResponseDto>>> GetTopComments(string contentId, int limit = 5)
        {
            var comments = await _commentsService.GetTopCommentsAsync(contentId, limit);
            var currentUserId = User.FindFirst("userId")?.Value;

            var responseDtos = new List<CommentResponseDto>();
            foreach (var comment in comments)
            {
                var dto = await MapToResponseDto(comment, currentUserId);
                responseDtos.Add(dto);
            }

            return Ok(responseDtos);
        }

        // Kullanıcının yorumları
        [HttpGet("user/{userId}")]
        [AllowAnonymous] // Herkese açık (profil görüntüleme)
        public async Task<ActionResult<List<CommentResponseDto>>> GetUserComments(string userId)
        {
            var comments = await _commentsService.GetCommentsByUserIdAsync(userId);
            var currentUserId = User.FindFirst("userId")?.Value;

            var responseDtos = new List<CommentResponseDto>();
            foreach (var comment in comments)
            {
                var dto = await MapToResponseDto(comment, currentUserId);
                responseDtos.Add(dto);
            }

            return Ok(responseDtos);
        }

        // Helper method - Comment'i ResponseDto'ya çevir
        private async Task<CommentResponseDto> MapToResponseDto(ContentComments comment, string? currentUserId)
        {
            // Kullanıcı adını getir (cache'lenebilir)
            var user = await _usersService.GetByIdAsync(comment.UserId);
            var userName = user?.UserName ?? "Bilinmeyen Kullanıcı";

            return new CommentResponseDto
            {
                Id = comment.Id,
                ContentId = comment.ContentId,
                UserId = comment.UserId,
                UserName = userName,
                Comment = comment.Comment,
                ParentCommentId = comment.ParentCommentId,
                LikeCount = comment.LikeCount,
                DislikeCount = comment.DislikeCount,
                InsertTime = comment.InsertTime,
                UpdateTime = comment.UpdateTime,
                IsOwner = comment.UserId == currentUserId
            };
        }
    }
}